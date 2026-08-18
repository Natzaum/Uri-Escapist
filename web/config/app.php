<?php

declare(strict_types=1);

$config = [
    'app' => [
        'name' => getenv('APP_NAME') ?: 'URI Escapist',
        'base_url' => rtrim(getenv('APP_BASE_URL') ?: '', '/'),
        'timezone' => getenv('APP_TIMEZONE') ?: 'America/Sao_Paulo',
        'session_name' => 'uri_escapist_admin',
    ],
    'database' => [
        'host' => getenv('DB_HOST') ?: '127.0.0.1',
        'port' => (int) (getenv('DB_PORT') ?: 3306),
        'name' => getenv('DB_NAME') ?: 'uri_escapist',
        'user' => getenv('DB_USER') ?: 'root',
        'password' => getenv('DB_PASSWORD') ?: '',
        'charset' => 'utf8mb4',
    ],
];

$localConfig = __DIR__ . '/local.php';

if (is_file($localConfig)) {
    $overrides = require $localConfig;

    if (is_array($overrides)) {
        $config = array_replace_recursive($config, $overrides);
    }
}

return $config;
