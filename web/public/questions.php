<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_auth();

$teacherId = (int) current_teacher()['id'];
$search = trim((string) ($_GET['search'] ?? ''));
$disciplineId = max(0, (int) ($_GET['discipline'] ?? 0));
$floorId = max(0, (int) ($_GET['floor'] ?? 0));
$status = (string) ($_GET['status'] ?? '');

$where = ['q.teacher_id = :teacher_id'];
$parameters = ['teacher_id' => $teacherId];

if ($search !== '') {
    $where[] = 'q.prompt LIKE :search';
    $parameters['search'] = '%' . $search . '%';
}

if ($disciplineId > 0) {
    $where[] = 'q.discipline_id = :discipline_id';
    $parameters['discipline_id'] = $disciplineId;
}

if ($floorId > 0) {
    $where[] = 'q.floor_id = :floor_id';
    $parameters['floor_id'] = $floorId;
}

if (in_array($status, ['draft', 'published'], true)) {
    $where[] = 'q.status = :status';
    $parameters['status'] = $status;
}

$statement = db()->prepare(
    "SELECT q.id, q.prompt, q.status, q.difficulty, q.correct_index,
            q.option_a, q.option_b, q.option_c, q.option_d, q.updated_at,
            d.name AS discipline, COALESCE(f.name, 'Todos os andares') AS floor
     FROM questions q
     INNER JOIN disciplines d ON d.id = q.discipline_id
     LEFT JOIN floors f ON f.id = q.floor_id
     WHERE " . implode(' AND ', $where) . "
     ORDER BY q.updated_at DESC
     LIMIT 100"
);
$statement->execute($parameters);
$questions = $statement->fetchAll();

$disciplines = db()->query('SELECT id, name FROM disciplines ORDER BY active DESC, name')->fetchAll();
$floors = db()->query('SELECT id, name FROM floors ORDER BY active DESC, id')->fetchAll();

render('questions/index', [
    'pageTitle' => 'Perguntas',
    'activePage' => 'questions',
    'questions' => $questions,
    'disciplines' => $disciplines,
    'floors' => $floors,
    'filters' => [
        'search' => $search,
        'discipline' => $disciplineId,
        'floor' => $floorId,
        'status' => $status,
    ],
]);
