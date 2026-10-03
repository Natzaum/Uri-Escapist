<?php

declare(strict_types=1);

if (($argv[1] ?? '') === '--request') {
    $_GET = json_decode($argv[3], true, 512, JSON_THROW_ON_ERROR);
    $_SERVER['REQUEST_METHOD'] = $_GET['method'] ?? 'GET';
    register_shutdown_function(static function (): void {
        fwrite(STDERR, (string) (http_response_code() ?: 200));
    });
    require $argv[2];
    exit;
}

require __DIR__ . '/question_selection_test.php';
$temp = sys_get_temp_dir() . '/uri-api-test-' . bin2hex(random_bytes(8));
mkdir($temp . '/public/api/v1', 0777, true);
mkdir($temp . '/src');
copy(dirname(__DIR__) . '/public/api/v1/questions.php', $temp . '/public/api/v1/questions.php');
copy(dirname(__DIR__) . '/src/question_selection.php', $temp . '/src/question_selection.php');
copy(__DIR__ . '/api_fixture.php', $temp . '/src/bootstrap.php');

function request(array $query): array
{
    global $temp;
    $process = proc_open([PHP_BINARY, __FILE__, '--request', $temp . '/public/api/v1/questions.php',
        json_encode($query, JSON_THROW_ON_ERROR)], [1 => ['pipe', 'w'], 2 => ['pipe', 'w']], $pipes);
    $body = stream_get_contents($pipes[1]);
    $status = stream_get_contents($pipes[2]);
    fclose($pipes[1]);
    fclose($pipes[2]);
    check(proc_close($process) === 0, 'Endpoint process failed: ' . $status);
    return [(int) $status, $body === '' ? [] : json_decode($body, true, 512, JSON_THROW_ON_ERROR)];
}

try {
    foreach (['facil', 'normal', 'dificil'] as $mode) {
        foreach ([1, 2] as $floor) {
            [$status, $body] = request(['scene' => "andar$floor", 'mode' => $mode, 'limit' => 10]);
            check($status === 200 && $body['success'], "HTTP failure: $mode/$floor");
            $rows = $body['data'];
            check(count($rows) === 10 && count(array_unique(array_column($rows, 'id'))) === 10, 'Book count / uniqueness');
            $allowed = question_difficulties($mode, "andar-$floor");
            check(array_diff(array_column($rows, 'difficulty'), $allowed) === [], 'Wrong difficulty');
            foreach ($rows as $row) {
                check($row['id'] < 100 && $row['correctIndex'] === $row['id'] % 4, 'Publication / answer mapping');
            }
            if (count($allowed) === 2) {
                check(count(array_unique(array_column($rows, 'difficulty'))) === 2, 'Missing mixed category');
            }
        }
    }
    foreach ([
        [['scene' => 'andar1', 'mode' => 'bad'], 422],
        [['scene' => 'andar3'], 422],
        [['scene' => 'unknown'], 422],
        [['scene' => 'andar1', 'fixture' => 'shortage'], 409],
        [['scene' => 'andar2', 'mode' => 'facil', 'fixture' => 'missing-medium'], 409],
        [['method' => 'POST'], 405],
        [['method' => 'OPTIONS'], 204],
    ] as [$query, $expectedStatus]) {
        [$status] = request($query);
        check($status === $expectedStatus, 'Unexpected error status: ' . json_encode($query));
    }
    [$status, $body] = request(['discipline' => 'geral', 'random' => 0, 'limit' => 10]);
    check($status === 200 && array_column($body['data'], 'id') === range(1, 10), 'Discipline compatibility');
    foreach (['facil', 'normal', 'dificil'] as $mode) {
        $used = [];
        foreach ([1, 2, 3] as $floor) {
            [$status, $body] = request(['scene' => "andar$floor", 'mode' => $mode,
                'limit' => 4, 'exclude' => implode(',', $used), 'fixture' => 'third-floor']);
            check($status === 200, "Fresh questions missing for $mode floor $floor");
            $ids = array_column($body['data'], 'id');
            check(array_intersect($used, $ids) === [], 'Question repeated across floors');
            $used = array_merge($used, $ids);
        }
        check(count(array_unique($used)) === 12, 'Three floors must have distinct questions');
    }
    foreach (['1 OR 1=1', '0', '-1', '1,,2', '2147483648', ['1']] as $invalid) {
        [$status] = request(['scene' => 'andar1', 'exclude' => $invalid]);
        check($status === 422, 'Invalid exclusion accepted');
    }
    [$status] = request(['scene' => 'andar1', 'exclude' => implode(',', range(1, 12))]);
    check($status === 409, 'Exhaustion must fail without repeating questions');
    [$status, $body] = request(['scene' => 'andar2', 'mode' => 'facil', 'exclude' => implode(',', range(1, 12))]);
    check($status === 200 && array_unique(array_column($body['data'], 'difficulty')) === ['media'], 'Use remaining eligible category');
    echo "PASS: endpoint SQL/JSON against SQLite fixtures, six playable combinations, errors and compatibility.\n";
} finally {
    foreach (['public/api/v1/questions.php', 'src/question_selection.php', 'src/bootstrap.php'] as $file) {
        unlink($temp . '/' . $file);
    }
    foreach (['public/api/v1', 'public/api', 'public', 'src', ''] as $directory) {
        rmdir($temp . '/' . $directory);
    }
}
