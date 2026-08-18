<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_auth();

if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}

verify_csrf();

$teacherId = (int) current_teacher()['id'];
$questionId = max(0, (int) ($_POST['id'] ?? 0));
$input = [
    'discipline_id' => max(0, (int) ($_POST['discipline_id'] ?? 0)),
    'prompt' => trim((string) ($_POST['prompt'] ?? '')),
    'option_a' => trim((string) ($_POST['option_a'] ?? '')),
    'option_b' => trim((string) ($_POST['option_b'] ?? '')),
    'option_c' => trim((string) ($_POST['option_c'] ?? '')),
    'option_d' => trim((string) ($_POST['option_d'] ?? '')),
    'correct_index' => (int) ($_POST['correct_index'] ?? -1),
    'difficulty' => (string) ($_POST['difficulty'] ?? ''),
    'status' => (string) ($_POST['status'] ?? ''),
];

remember_input($input);
$errors = [];

if ($input['discipline_id'] < 1) {
    $errors[] = 'Selecione uma disciplina.';
}

if ($input['prompt'] === '' || mb_strlen($input['prompt']) > 500) {
    $errors[] = 'O enunciado deve conter entre 1 e 500 caracteres.';
}

foreach (['option_a', 'option_b', 'option_c', 'option_d'] as $field) {
    if ($input[$field] === '' || mb_strlen($input[$field]) > 255) {
        $errors[] = 'Preencha as quatro alternativas com até 255 caracteres.';
        break;
    }
}

$normalizedOptions = array_map(
    static fn (string $option): string => mb_strtolower(trim($option), 'UTF-8'),
    [$input['option_a'], $input['option_b'], $input['option_c'], $input['option_d']]
);

if (count(array_unique($normalizedOptions)) !== 4) {
    $errors[] = 'As quatro alternativas precisam ser diferentes.';
}

if ($input['correct_index'] < 0 || $input['correct_index'] > 3) {
    $errors[] = 'Marque qual alternativa é a correta.';
}

if (!in_array($input['difficulty'], ['facil', 'media', 'dificil'], true)) {
    $errors[] = 'Selecione uma dificuldade válida.';
}

if (!in_array($input['status'], ['draft', 'published'], true)) {
    $errors[] = 'Selecione um status válido.';
}

$disciplineStatement = db()->prepare('SELECT COUNT(*) FROM disciplines WHERE id = :id');
$disciplineStatement->execute(['id' => $input['discipline_id']]);

if ((int) $disciplineStatement->fetchColumn() !== 1) {
    $errors[] = 'A disciplina selecionada não existe.';
}

if ($questionId > 0) {
    $ownerStatement = db()->prepare('SELECT COUNT(*) FROM questions WHERE id = :id AND teacher_id = :teacher_id');
    $ownerStatement->execute(['id' => $questionId, 'teacher_id' => $teacherId]);

    if ((int) $ownerStatement->fetchColumn() !== 1) {
        $errors[] = 'Pergunta não encontrada ou sem permissão para edição.';
    }
}

if ($errors !== []) {
    foreach (array_unique($errors) as $error) {
        flash('error', $error);
    }

    redirect('/question-form.php' . ($questionId > 0 ? '?id=' . $questionId : ''));
}

$parameters = $input + ['teacher_id' => $teacherId];

if ($questionId > 0) {
    $parameters['id'] = $questionId;
    $statement = db()->prepare(
        'UPDATE questions SET
            discipline_id = :discipline_id,
            prompt = :prompt,
            option_a = :option_a,
            option_b = :option_b,
            option_c = :option_c,
            option_d = :option_d,
            correct_index = :correct_index,
            difficulty = :difficulty,
            status = :status
         WHERE id = :id AND teacher_id = :teacher_id'
    );
    $message = 'Pergunta atualizada com sucesso.';
} else {
    $statement = db()->prepare(
        'INSERT INTO questions (
            discipline_id, teacher_id, prompt,
            option_a, option_b, option_c, option_d,
            correct_index, difficulty, status
         ) VALUES (
            :discipline_id, :teacher_id, :prompt,
            :option_a, :option_b, :option_c, :option_d,
            :correct_index, :difficulty, :status
         )'
    );
    $message = 'Pergunta cadastrada com sucesso.';
}

$statement->execute($parameters);
clear_old_input();
flash('success', $message);
redirect('/questions.php');
