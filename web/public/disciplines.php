<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_auth();

$disciplines = db()->query(
    "SELECT d.id, d.name, d.slug, d.active,
            COUNT(q.id) AS question_count,
            SUM(q.status = 'published') AS published_count
     FROM disciplines d
     LEFT JOIN questions q ON q.discipline_id = d.id
     GROUP BY d.id, d.name, d.slug, d.active
     ORDER BY d.active DESC, d.name"
)->fetchAll();

render('disciplines', [
    'pageTitle' => 'Disciplinas',
    'activePage' => 'disciplines',
    'disciplines' => $disciplines,
]);

clear_old_input();
