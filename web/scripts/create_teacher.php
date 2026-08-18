<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';

if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}

$arguments = getopt('', ['name:', 'email:', 'password:']);
$name = trim((string) ($arguments['name'] ?? ''));
$email = mb_strtolower(trim((string) ($arguments['email'] ?? '')), 'UTF-8');
$password = (string) ($arguments['password'] ?? '');

if ($name === '' || !filter_var($email, FILTER_VALIDATE_EMAIL) || strlen($password) < 8) {
    fwrite(
        STDERR,
        "Uso: php scripts/create_teacher.php --name=\"Nome\" --email=professor@exemplo.com --password=\"senha-com-8-caracteres\"\n"
    );
    exit(1);
}

$statement = db()->prepare(
    'INSERT INTO teachers (name, email, password_hash, active)
     VALUES (:name, :email, :password_hash, 1)
     ON DUPLICATE KEY UPDATE
        name = VALUES(name),
        password_hash = VALUES(password_hash),
        active = 1'
);
$statement->execute([
    'name' => $name,
    'email' => $email,
    'password_hash' => password_hash($password, PASSWORD_DEFAULT),
]);

fwrite(STDOUT, "Professor criado ou atualizado com sucesso: {$email}\n");
