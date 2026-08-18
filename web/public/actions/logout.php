<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_auth();

if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}

verify_csrf();
logout_teacher();
redirect('/login.php');
