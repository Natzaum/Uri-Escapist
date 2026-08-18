<?php

declare(strict_types=1);

define('API_REQUEST', true);
require dirname(__DIR__, 3) . '/src/bootstrap.php';

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Cache-Control: no-store, max-age=0');

try {
    db()->query('SELECT 1');
    http_response_code(200);
    echo json_encode([
        'success' => true,
        'service' => 'uri-escapist-questions',
        'database' => 'connected',
        'time' => date(DATE_ATOM),
    ], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
} catch (Throwable $exception) {
    error_log('[URI Escapist Health] ' . $exception->getMessage());
    http_response_code(503);
    echo json_encode([
        'success' => false,
        'service' => 'uri-escapist-questions',
        'database' => 'unavailable',
    ], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
}
