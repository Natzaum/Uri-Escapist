<?php

declare(strict_types=1);

define('API_REQUEST', true);
require dirname(__DIR__, 3) . '/src/bootstrap.php';

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

$discipline = trim((string) ($_GET['discipline'] ?? 'geral'));
$limit = min(50, max(1, (int) ($_GET['limit'] ?? 10)));
$randomOrder = (string) ($_GET['random'] ?? '1') !== '0';

if (!preg_match('/^[a-z0-9-]{1,120}$/', $discipline)) {
    json_response(['success' => false, 'message' => 'Chave de disciplina inválida.'], 422);
}

try {
    $orderBy = $randomOrder ? 'RAND()' : 'q.id ASC';
    $statement = db()->prepare(
        "SELECT q.id, q.prompt, q.option_a, q.option_b, q.option_c, q.option_d,
                q.correct_index, q.difficulty, d.name AS discipline_name, d.slug AS discipline_slug
         FROM questions q
         INNER JOIN disciplines d ON d.id = q.discipline_id
         WHERE q.status = 'published'
           AND d.active = 1
           AND d.slug = :discipline
         ORDER BY {$orderBy}
         LIMIT {$limit}"
    );
    $statement->execute(['discipline' => $discipline]);

    $questions = array_map(
        static fn (array $row): array => [
            'id' => (int) $row['id'],
            'discipline' => (string) $row['discipline_slug'],
            'disciplineName' => (string) $row['discipline_name'],
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
        $statement->fetchAll()
    );

    json_response([
        'success' => true,
        'data' => $questions,
        'meta' => [
            'discipline' => $discipline,
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
