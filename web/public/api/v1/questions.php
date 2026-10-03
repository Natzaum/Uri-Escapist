<?php

declare(strict_types=1);

define('API_REQUEST', true);
require dirname(__DIR__, 3) . '/src/bootstrap.php';
require dirname(__DIR__, 3) . '/src/question_selection.php';

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');
header('Cache-Control: no-store, max-age=0');

if (($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'OPTIONS') {
    http_response_code(204);
    exit;
}

if (($_SERVER['REQUEST_METHOD'] ?? 'GET') !== 'GET') {
    json_response(['success' => false, 'message' => 'Método não permitido.'], 405);
}

$mode = (string) ($_GET['mode'] ?? 'normal');
if (!in_array($mode, ['facil', 'normal', 'dificil'], true)) {
    json_response(['success' => false, 'message' => 'Modo inválido. Use facil, normal ou dificil.'], 422);
}

$scene = trim((string) ($_GET['scene'] ?? ''));
$discipline = trim((string) ($_GET['discipline'] ?? 'geral'));
$limit = min(50, max(1, (int) ($_GET['limit'] ?? 10)));
$randomOrder = (string) ($_GET['random'] ?? '1') !== '0';

if ($scene !== '' && !preg_match('/^[A-Za-z0-9_-]{1,120}$/', $scene)) {
    json_response(['success' => false, 'message' => 'Nome de cena inválido.'], 422);
}

if ($scene === '' && !preg_match('/^[a-z0-9-]{1,120}$/', $discipline)) {
    json_response(['success' => false, 'message' => 'Chave de disciplina inválida.'], 422);
}

try {
    $floor = null;
    $parameters = [];
    $difficulties = ['facil', 'media', 'dificil'];
    $contentFilter = 'd.slug = :discipline';

    if ($scene !== '') {
        $floorStatement = db()->prepare(
            'SELECT id, name, slug, scene_name FROM floors WHERE scene_name = :scene AND active = 1 LIMIT 1'
        );
        $floorStatement->execute(['scene' => $scene]);
        $floor = $floorStatement->fetch();

        if (!$floor) {
            json_response(['success' => false, 'message' => 'Cena sem andar ativo cadastrado.'], 422);
        }
        try {
            $difficulties = question_difficulties($mode, (string) $floor['slug']);
        } catch (InvalidArgumentException $exception) {
            json_response(['success' => false, 'message' => $exception->getMessage()], 422);
        }
        // Legacy floor assignments no longer constrain the shared question bank.
        $contentFilter = '1 = 1';
    } else {
        $parameters['discipline'] = $discipline;
    }

    $orderBy = $randomOrder ? 'RAND()' : 'q.id ASC';
    $statement = db()->prepare(
        "SELECT q.id, q.prompt, q.option_a, q.option_b, q.option_c, q.option_d,
                q.correct_index, q.difficulty,
                d.name AS discipline_name, d.slug AS discipline_slug
         FROM questions q
         INNER JOIN disciplines d ON d.id = q.discipline_id
         WHERE q.status = 'published'
           AND d.active = 1
           AND {$contentFilter}
           AND q.difficulty = :difficulty
         ORDER BY {$orderBy}
         LIMIT {$limit}"
    );
    $pools = [];
    foreach ($difficulties as $difficulty) {
        $statement->execute($parameters + ['difficulty' => $difficulty]);
        $pools[] = $statement->fetchAll();
    }
    $selected = select_questions($pools, $limit, $randomOrder);
    if (!$randomOrder) {
        if ($scene === '') {
            $selected = array_merge(...$pools);
        }
        usort($selected, static fn (array $a, array $b): int => (int) $a['id'] <=> (int) $b['id']);
        $selected = array_slice($selected, 0, $limit);
    }
    if ($scene !== '' && (count($selected) < $limit || in_array([], $pools, true))) {
        json_response([
            'success' => false,
            'message' => 'Perguntas insuficientes. Publique pelo menos ' . $limit
                . ' questões elegíveis (' . implode(' + ', $difficulties)
                . '), com pelo menos uma de cada dificuldade indicada.',
            'meta' => ['gameMode' => $mode, 'difficulties' => $difficulties,
                'available' => count($selected), 'required' => $limit],
        ], 409);
    }

    $questions = array_map(
        static fn (array $row): array => [
            'id' => (int) $row['id'],
            'discipline' => (string) $row['discipline_slug'],
            'disciplineName' => (string) $row['discipline_name'],
            'floor' => 'todos',
            'floorName' => 'Conforme a dificuldade da partida',
            'prompt' => (string) $row['prompt'],
            'options' => [
                (string) $row['option_a'],
                (string) $row['option_b'],
                (string) $row['option_c'],
                (string) $row['option_d'],
            ],
            'correctIndex' => (int) $row['correct_index'],
            'difficulty' => (string) $row['difficulty'],
        ],
        $selected
    );

    json_response([
        'success' => true,
        'data' => $questions,
        'meta' => [
            'gameMode' => $mode,
            'difficulties' => $difficulties,
            'mode' => $scene !== '' ? 'scene' : 'discipline',
            'scene' => $scene !== '' ? $scene : null,
            'floor' => $floor ? (string) $floor['slug'] : null,
            'floorName' => $floor ? (string) $floor['name'] : null,
            'discipline' => $scene === '' ? $discipline : null,
            'count' => count($questions),
            'generatedAt' => date(DATE_ATOM),
        ],
    ]);
} catch (Throwable $exception) {
    error_log('[URI Escapist API] ' . $exception->getMessage());
    json_response(['success' => false, 'message' => 'Não foi possível carregar as perguntas.'], 500);
}

function json_response(array $payload, int $status = 200): never
{
    http_response_code($status);
    echo json_encode($payload, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES | JSON_THROW_ON_ERROR);
    exit;
}
