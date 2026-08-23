<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_auth();

$teacher = current_teacher();
$teacherId = (int) $teacher['id'];

$summaryStatement = db()->prepare(
    "SELECT
        COUNT(*) AS total,
        SUM(status = 'published') AS published,
        SUM(status = 'draft') AS drafts
     FROM questions
     WHERE teacher_id = :teacher_id"
);
$summaryStatement->execute(['teacher_id' => $teacherId]);
$summary = $summaryStatement->fetch() ?: ['total' => 0, 'published' => 0, 'drafts' => 0];

$disciplinesCount = (int) db()->query('SELECT COUNT(*) FROM disciplines WHERE active = 1')->fetchColumn();

$recentStatement = db()->prepare(
    "SELECT q.id, q.prompt, q.status, q.difficulty, q.updated_at,
            d.name AS discipline, COALESCE(f.name, 'Todos os andares') AS floor
     FROM questions q
     INNER JOIN disciplines d ON d.id = q.discipline_id
     LEFT JOIN floors f ON f.id = q.floor_id
     WHERE q.teacher_id = :teacher_id
     ORDER BY q.updated_at DESC
     LIMIT 6"
);
$recentStatement->execute(['teacher_id' => $teacherId]);
$recentQuestions = $recentStatement->fetchAll();

render('dashboard', [
    'pageTitle' => 'Visão geral',
    'activePage' => 'dashboard',
    'summary' => $summary,
    'disciplinesCount' => $disciplinesCount,
    'recentQuestions' => $recentQuestions,
]);
