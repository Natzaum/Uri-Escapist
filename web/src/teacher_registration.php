<?php

declare(strict_types=1);

function register_teacher(string $name, string $email, string $password): void
{
    $name = trim($name);
    $email = mb_strtolower(trim($email), 'UTF-8');
    if ($name === '' || mb_strlen($name, 'UTF-8') > 120) {
        throw new InvalidArgumentException('Informe um nome com até 120 caracteres.');
    }
    if (strlen($email) > 190 || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
        throw new InvalidArgumentException('Informe um e-mail válido com até 190 caracteres.');
    }
    if (mb_strlen($password, 'UTF-8') < 8 || strlen($password) > 72 || str_contains($password, "\0")) {
        throw new InvalidArgumentException('A senha deve ter pelo menos 8 caracteres e no máximo 72 bytes.');
    }

    $statement = db()->prepare(
        'INSERT INTO teachers (name, email, password_hash, active) VALUES (:name, :email, :password_hash, 1)'
    );
    $statement->execute([
        'name' => $name,
        'email' => $email,
        'password_hash' => password_hash($password, PASSWORD_DEFAULT),
    ]);
}
