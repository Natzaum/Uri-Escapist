<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/teacher_registration.php';

function db(): PDO
{
    static $connection;
    if (!$connection) {
        $connection = new PDO('sqlite::memory:', null, null, [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]);
        $connection->exec('CREATE TABLE teachers (name TEXT, email TEXT UNIQUE, password_hash TEXT, active INTEGER)');
    }
    return $connection;
}

function check(bool $condition, string $message): void
{
    if (!$condition) throw new RuntimeException($message);
}

register_teacher('  Natã Cezer Bordignon  ', '  PROFESSOR@example.com ', 'senha-teste-123');
$teacher = db()->query('SELECT * FROM teachers')->fetch(PDO::FETCH_ASSOC);
check($teacher['name'] === 'Natã Cezer Bordignon', 'Name normalization');
check($teacher['email'] === 'professor@example.com', 'Email normalization');
check(password_verify('senha-teste-123', $teacher['password_hash']), 'Login-compatible password hash');
check($teacher['password_hash'] !== 'senha-teste-123' && (int) $teacher['active'] === 1, 'Hash and active account');
try {
    register_teacher('Outra pessoa', 'professor@example.com', 'outra-senha');
    throw new RuntimeException('Duplicate email accepted');
} catch (PDOException) {
    check(db()->query('SELECT password_hash FROM teachers')->fetchColumn() === $teacher['password_hash'], 'Existing password changed');
}
foreach ([['', 'a@b.com', '12345678'], [str_repeat('a', 121), 'a@b.com', '12345678'],
    ['Nome', 'invalido', '12345678'], ['Nome', 'a@b.com', '123'],
    ['Nome', 'a@b.com', str_repeat('a', 73)], ['Nome', 'a@b.com', "12345678\0"]] as $input) {
    try {
        register_teacher(...$input);
        throw new RuntimeException('Invalid input accepted');
    } catch (InvalidArgumentException) {
        // Expected.
    }
}
check((int) db()->query('SELECT COUNT(*) FROM teachers')->fetchColumn() === 1, 'Invalid accounts stored');
echo "PASS: registration, normalization, password verification, duplicates and validation.\n";
