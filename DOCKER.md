# Executando o URI Escapist com Docker

O ambiente Docker contém tudo que o painel web precisa:

- **app**: PHP 8.3 com Apache, `pdo_mysql` e `mbstring`;
- **db**: MySQL 8.4 com armazenamento persistente;
- **phpmyadmin**: administração visual do banco;
- criação automática do banco, das tabelas e da disciplina `geral` no primeiro início.

## 1. Preparar o ambiente

Instale e abra o Docker Desktop. No PowerShell, dentro da raiz do projeto, crie seu arquivo de configuração:

```powershell
Copy-Item .env.example .env
notepad .env
```

Troque `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` e `INITIAL_TEACHER_PASSWORD`. O arquivo `.env` não é versionado.

## 2. Iniciar os containers

```powershell
docker compose up -d --build
```

Confira se os três serviços estão ativos:

```powershell
docker compose ps
```

Na primeira execução, o download das imagens e a compilação da extensão PHP podem levar alguns minutos.

## 3. Entrar com o primeiro professor

O professor informado nas variáveis `INITIAL_TEACHER_NAME`, `INITIAL_TEACHER_EMAIL` e `INITIAL_TEACHER_PASSWORD` é criado automaticamente no primeiro início. O container não altera essa conta quando ela já existe.

Para adicionar outro professor ou redefinir uma senha manualmente, execute:

```powershell
docker compose exec app php scripts/create_teacher.php --name="Professor" --email="professor@uri.edu.br" --password="uma-senha-com-8-ou-mais-caracteres"
```

O comando também permite redefinir a senha caso seja executado novamente com o mesmo e-mail.

## 4. Acessar

| Serviço | Endereço | Credenciais |
|---|---|---|
| Painel do professor | `http://127.0.0.1:8000/login.php` | `INITIAL_TEACHER_EMAIL` e `INITIAL_TEACHER_PASSWORD` do `.env` |
| phpMyAdmin | `http://127.0.0.1:8081` | Usuário e senha do `.env` |
| API para Unity | `http://127.0.0.1:8000/api/v1/questions.php?scene=andar1&limit=10` | Leitura pública |
| Saúde da API | `http://127.0.0.1:8000/api/v1/health.php` | — |

No phpMyAdmin, use o valor de `MYSQL_USER` como usuário e `MYSQL_PASSWORD` como senha. O servidor já está definido internamente como `db`.

A URL padrão configurada no `BookManager` da Unity já aponta para a porta `8000`. Se alterar `APP_PORT` no `.env`, atualize também **Questions Api Url** no Inspector.

## Acessar por outro computador na mesma rede

O painel e o phpMyAdmin escutam em `0.0.0.0`, portanto podem ser acessados pelo IP da máquina que executa o Docker. O MySQL na porta `3307` continua restrito ao computador servidor; o acesso visual ao banco deve ser feito pelo phpMyAdmin.

No computador servidor, abra o PowerShell **como Administrador** e execute:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\habilitar-acesso-rede.ps1
```

O script inicia os containers, libera as portas publicadas do painel e do phpMyAdmin no Firewall do Windows para redes privadas e mostra os endereços de acesso. No outro computador, use, por exemplo:

```text
http://192.168.0.15:8000
http://192.168.0.15:8081
```

Substitua `192.168.0.15` pelo IP mostrado pelo script. Use `http://`, e não `https://`. Os dois computadores precisam estar na mesma rede, e a conexão do Windows deve estar marcada como **Rede privada**.

Para a Unity executada em outro computador, altere **Questions Api Url** nos dois `BookManager` para:

```text
http://IP-DO-SERVIDOR:8000/api/v1/questions.php
```

Não encaminhe as portas `8000` ou `8081` no roteador e não exponha este ambiente de desenvolvimento diretamente à internet.

## Operação diária

Parar sem excluir os containers:

```powershell
docker compose stop
```

Iniciar novamente:

```powershell
docker compose start
```

Ver os logs:

```powershell
docker compose logs -f app db phpmyadmin
```

Remover os containers mantendo os dados do MySQL:

```powershell
docker compose down
```

## Recriar o banco do zero

> Atenção: o comando abaixo apaga permanentemente professores, disciplinas e perguntas cadastradas.

```powershell
docker compose down -v
docker compose up -d --build
```

O arquivo [`web/database/schema.sql`](web/database/schema.sql) é executado automaticamente somente quando o volume do MySQL está vazio.

## Problemas comuns

### `ERR_CONNECTION_REFUSED` na porta 8081

Primeiro confirme que o Docker Desktop está aberto e que o phpMyAdmin está em execução:

```powershell
docker compose up -d --build
docker compose ps
docker compose logs --tail=100 phpmyadmin db
```

O resultado de `docker compose ps` deve mostrar algo equivalente a `0.0.0.0:8081->80/tcp` para o phpMyAdmin. Teste primeiro `http://127.0.0.1:8081` no computador servidor. Se funcionar localmente, mas não pelo IP, execute `scripts/habilitar-acesso-rede.ps1` como Administrador para configurar o firewall.

### Porta ocupada

Altere as portas no `.env`. Exemplo:

```dotenv
APP_PORT=8001
PHPMYADMIN_PORT=8082
MYSQL_PORT=3308
```

### Conferir o estado do banco

```powershell
docker compose exec db mysqladmin ping -h 127.0.0.1 -u root -p
```

### Recriar apenas a imagem PHP após mudar o código

```powershell
docker compose up -d --build app
```

O container executa automaticamente as migrações pendentes antes de iniciar o Apache. Atualizações de estrutura preservam as perguntas e os professores já cadastrados.
