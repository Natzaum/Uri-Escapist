<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_once WEB_ROOT . '/src/registration_auth.php';
if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}
verify_csrf();
logout_registration_admin();
redirect('/registro-login.php');
