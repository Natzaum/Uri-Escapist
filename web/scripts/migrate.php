<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';

if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}

$connection = db();
$connection->exec(
    'CREATE TABLE IF NOT EXISTS schema_migrations (
        name VARCHAR(190) PRIMARY KEY,
        applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB'
);

$migrationFiles = glob(dirname(__DIR__) . '/database/migrations/*.sql') ?: [];
sort($migrationFiles, SORT_STRING);

$wasApplied = $connection->prepare('SELECT COUNT(*) FROM schema_migrations WHERE name = :name');
$recordMigration = $connection->prepare('INSERT INTO schema_migrations (name) VALUES (:name)');

foreach ($migrationFiles as $migrationFile) {
    $migrationName = basename($migrationFile);
    $wasApplied->execute(['name' => $migrationName]);

    if ((int) $wasApplied->fetchColumn() > 0) {
        fwrite(STDOUT, "Migração já aplicada: {$migrationName}\n");
        continue;
    }

    $sql = file_get_contents($migrationFile);

    if ($sql === false || trim($sql) === '') {
        throw new RuntimeException("Migração vazia ou ilegível: {$migrationName}");
    }

    $statements = preg_split('/;\s*(?:\r?\n|$)/', trim($sql)) ?: [];

    foreach ($statements as $statement) {
        if (trim($statement) !== '') {
            $connection->exec($statement);
        }
    }

    $recordMigration->execute(['name' => $migrationName]);
    fwrite(STDOUT, "Migração aplicada: {$migrationName}\n");
}
