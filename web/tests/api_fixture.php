<?php

// Isolated SQLite fixture for exercising the actual endpoint without the remote database.
declare(strict_types=1);

function db(): PDO
{
    static $connection;
    if ($connection instanceof PDO) {
        return $connection;
    }
    $connection = new PDO('sqlite::memory:', null, null, [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    ]);
    $connection->sqliteCreateFunction('RAND', fn () => random_int(0, PHP_INT_MAX));
    $connection->exec("CREATE TABLE floors (id INTEGER, name TEXT, slug TEXT, scene_name TEXT, active INTEGER);
        INSERT INTO floors VALUES (1,'Andar 1','andar-1','andar1',1),(2,'Andar 2','andar-2','andar2',1),
        (3,'Andar 3','andar-3','andar3',0);
        CREATE TABLE disciplines (id INTEGER, name TEXT, slug TEXT, active INTEGER);
        INSERT INTO disciplines VALUES (1,'Geral','geral',1),(2,'Inativa','inativa',0);
        CREATE TABLE questions (id INTEGER, discipline_id INTEGER, floor_id INTEGER, prompt TEXT,
        option_a TEXT, option_b TEXT, option_c TEXT, option_d TEXT, correct_index INTEGER,
        difficulty TEXT, status TEXT)");
    $insert = $connection->prepare('INSERT INTO questions VALUES (?, ?, 99, ?, ?, ?, ?, ?, ?, ?, ?)');
    for ($id = 1; $id <= 36; $id++) {
        $difficulty = ['facil', 'media', 'dificil'][intdiv($id - 1, 12)];
        $insert->execute([$id, 1, "Pergunta $id", 'A', 'B', 'C', 'D', $id % 4, $difficulty, 'published']);
    }
    $insert->execute([100, 1, 'Rascunho', 'A', 'B', 'C', 'D', 0, 'facil', 'draft']);
    $insert->execute([101, 2, 'Disciplina inativa', 'A', 'B', 'C', 'D', 0, 'facil', 'published']);
    if (($_GET['fixture'] ?? '') === 'shortage') {
        $connection->exec('DELETE FROM questions WHERE id > 2');
    }
    if (($_GET['fixture'] ?? '') === 'missing-medium') {
        $connection->exec("DELETE FROM questions WHERE difficulty = 'media'");
    }
    return $connection;
}
