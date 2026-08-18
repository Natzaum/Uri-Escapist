<?php

declare(strict_types=1);

define('WEB_ROOT', dirname(__DIR__));

require_once __DIR__ . '/helpers.php';

$GLOBALS['app_config'] = require WEB_ROOT . '/config/app.php';

date_default_timezone_set((string) config('app.timezone', 'America/Sao_Paulo'));

require_once __DIR__ . '/database.php';
require_once __DIR__ . '/auth.php';

if (PHP_SAPI !== 'cli' && !defined('API_REQUEST') && session_status() !== PHP_SESSION_ACTIVE) {
    session_name((string) config('app.session_name', 'uri_escapist_admin'));
    session_set_cookie_params([
        'lifetime' => 0,
        'path' => '/',
        'secure' => isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off',
        'httponly' => true,
        'samesite' => 'Lax',
    ]);
    session_start();
}
