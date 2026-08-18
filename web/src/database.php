<?php

declare(strict_types=1);

function db(): PDO
{
    static $connection = null;

    if ($connection instanceof PDO) {
        return $connection;
    }

    $database = (array) config('database', []);
    $dsn = sprintf(
        'mysql:host=%s;port=%d;dbname=%s;charset=%s',
        $database['host'] ?? '127.0.0.1',
        $database['port'] ?? 3306,
        $database['name'] ?? 'uri_escapist',
        $database['charset'] ?? 'utf8mb4'
    );

    $connection = new PDO(
        $dsn,
        (string) ($database['user'] ?? 'root'),
        (string) ($database['password'] ?? ''),
        [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES => false,
        ]
    );

    return $connection;
}
