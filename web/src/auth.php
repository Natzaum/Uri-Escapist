<?php

declare(strict_types=1);

function current_teacher(): ?array
{
    $teacher = $_SESSION['teacher'] ?? null;

    return is_array($teacher) ? $teacher : null;
}

function require_auth(): void
{
    if (current_teacher() === null) {
        flash('warning', 'Entre com sua conta para acessar o painel.');
        redirect('/login.php');
    }
}

function attempt_login(string $email, string $password): bool
{
    $statement = db()->prepare(
        'SELECT id, name, email, password_hash FROM teachers WHERE email = :email AND active = 1 LIMIT 1'
    );
    $statement->execute(['email' => mb_strtolower(trim($email), 'UTF-8')]);
    $teacher = $statement->fetch();

    if (!$teacher || !password_verify($password, (string) $teacher['password_hash'])) {
        return false;
    }

    session_regenerate_id(true);
    $_SESSION['teacher'] = [
        'id' => (int) $teacher['id'],
        'name' => (string) $teacher['name'],
        'email' => (string) $teacher['email'],
    ];

    return true;
}

function logout_teacher(): void
{
    unset($_SESSION['teacher'], $_SESSION['_csrf'], $_SESSION['_old']);
    session_regenerate_id(true);
}
