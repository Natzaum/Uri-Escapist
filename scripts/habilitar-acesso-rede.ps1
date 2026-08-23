[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
$isAdministrator = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdministrator) {
    throw 'Abra o PowerShell como Administrador e execute este script novamente.'
}

if ($null -eq (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker nao encontrado. Instale e abra o Docker Desktop antes de continuar.'
}

$projectRoot = Split-Path -Parent $PSScriptRoot

Push-Location $projectRoot
try {
    & docker compose up -d --build
    if ($LASTEXITCODE -ne 0) {
        throw 'Nao foi possivel iniciar os containers.'
    }

    function Get-PublishedPort {
        param([Parameter(Mandatory)][string] $Service)

        $mapping = (& docker compose port $Service 80 | Select-Object -First 1).Trim()
        if ($LASTEXITCODE -ne 0 -or $mapping -notmatch ':(\d+)$') {
            throw "Nao foi possivel descobrir a porta publicada pelo servico $Service."
        }

        return [int] $Matches[1]
    }

    $appPort = Get-PublishedPort -Service 'app'
    $phpMyAdminPort = Get-PublishedPort -Service 'phpmyadmin'
}
finally {
    Pop-Location
}

$rules = @(
    [PSCustomObject]@{
        DisplayName = "URI Escapist - Painel e API TCP $appPort"
        Port = $appPort
    },
    [PSCustomObject]@{
        DisplayName = "URI Escapist - phpMyAdmin TCP $phpMyAdminPort"
        Port = $phpMyAdminPort
    }
)

foreach ($rule in $rules) {
    $existingRule = Get-NetFirewallRule -DisplayName $rule.DisplayName -ErrorAction SilentlyContinue

    if ($null -eq $existingRule) {
        New-NetFirewallRule `
            -DisplayName $rule.DisplayName `
            -Direction Inbound `
            -Action Allow `
            -Protocol TCP `
            -LocalPort $rule.Port `
            -Profile Private | Out-Null
    }
    else {
        $existingRule | Set-NetFirewallRule -Enabled True -Action Allow -Profile Private
    }
}

$publicProfiles = Get-NetConnectionProfile -ErrorAction SilentlyContinue |
    Where-Object { $_.IPv4Connectivity -ne 'Disconnected' -and $_.NetworkCategory -eq 'Public' }

if ($publicProfiles) {
    Write-Warning 'A rede ativa esta como Publica. Altere-a para Privada nas configuracoes de Rede e Internet do Windows.'
}

$addresses = Get-NetIPConfiguration -ErrorAction SilentlyContinue |
    Where-Object { $null -ne $_.IPv4DefaultGateway -and $null -ne $_.IPv4Address } |
    ForEach-Object { $_.IPv4Address.IPAddress } |
    Where-Object { $_ -notmatch '^(127\.|169\.254\.)' } |
    Sort-Object -Unique

Write-Host ''
Write-Host 'Acesso local:'
Write-Host "  Painel:      http://127.0.0.1:$appPort"
Write-Host "  phpMyAdmin:  http://127.0.0.1:$phpMyAdminPort"

foreach ($address in $addresses) {
    Write-Host ''
    Write-Host "Acesso pela rede usando $address`:"
    Write-Host "  Painel:      http://$address`:$appPort"
    Write-Host "  phpMyAdmin:  http://$address`:$phpMyAdminPort"
}

Write-Host ''
Write-Host 'As regras criadas aceitam conexoes somente em redes marcadas como Privadas no Windows.'
