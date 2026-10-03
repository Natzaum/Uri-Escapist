<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_auth();

$teacherId = (int) current_teacher()['id'];
$questionId = max(0, (int) ($_GET['id'] ?? 0));
$question = [
    'id' => 0,
    'discipline_id' => '',
    'prompt' => '',
    'option_a' => '',
    'option_b' => '',
    'option_c' => '',
    'option_d' => '',
    'correct_index' => 0,
    'difficulty' => 'media',
    'status' => 'draft',
];

if ($questionId > 0) {
    $statement = db()->prepare('SELECT * FROM questions WHERE id = :id AND teacher_id = :teacher_id LIMIT 1');
    $statement->execute(['id' => $questionId, 'teacher_id' => $teacherId]);
    $storedQuestion = $statement->fetch();

    if (!$storedQuestion) {
        flash('error', 'Pergunta não encontrada ou sem permissão para edição.');
        redirect('/questions.php');
    }

    $question = $storedQuestion;
}

$disciplines = db()->query('SELECT id, name, active FROM disciplines ORDER BY active DESC, name')->fetchAll();

render('questions/form', [
    'pageTitle' => $questionId > 0 ? 'Editar pergunta' : 'Nova pergunta',
    'activePage' => 'questions',
    'question' => $question,
    'disciplines' => $disciplines,
]);

clear_old_input();
