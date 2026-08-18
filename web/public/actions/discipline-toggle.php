<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_auth();

if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}

verify_csrf();

$statement = db()->prepare('UPDATE disciplines SET active = NOT active WHERE id = :id');
$statement->execute(['id' => max(0, (int) ($_POST['id'] ?? 0))]);

flash(
    $statement->rowCount() === 1 ? 'success' : 'error',
    $statement->rowCount() === 1 ? 'Status da disciplina atualizado.' : 'Disciplina não encontrada.'
);
redirect('/disciplines.php');
