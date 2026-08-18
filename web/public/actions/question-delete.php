<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_auth();

if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}

verify_csrf();

$statement = db()->prepare('DELETE FROM questions WHERE id = :id AND teacher_id = :teacher_id');
$statement->execute([
    'id' => max(0, (int) ($_POST['id'] ?? 0)),
    'teacher_id' => (int) current_teacher()['id'],
]);

flash(
    $statement->rowCount() === 1 ? 'success' : 'error',
    $statement->rowCount() === 1 ? 'Pergunta excluída.' : 'Pergunta não encontrada ou sem permissão.'
);
redirect('/questions.php');
