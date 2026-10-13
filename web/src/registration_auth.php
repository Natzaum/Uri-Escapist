<?php

declare(strict_types=1);

function can_register_teachers(): bool
{
    return ($_SESSION['registration_admin'] ?? false) === true;
}

function attempt_registration_login(string $username, string $password): bool
{
    if (!hash_equals('nata', $username) || !hash_equals('12345678', $password)) {
        return false;
    }
    session_regenerate_id(true);
    $_SESSION['registration_admin'] = true;
    return true;
}

function require_registration_admin(): void
{
    if (!can_register_teachers()) {
        flash('warning', 'Entre com o acesso de cadastro para registrar professores.');
        redirect('/registro-login.php');
    }
}

function logout_registration_admin(): void
{
    unset($_SESSION['registration_admin'], $_SESSION['_csrf'], $_SESSION['_old']);
    session_regenerate_id(true);
}
