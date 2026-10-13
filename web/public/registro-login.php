<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_once WEB_ROOT . '/src/registration_auth.php';

if (can_register_teachers()) redirect('/teachers.php');

if (is_post()) {
    verify_csrf();
    $username = is_string($_POST['username'] ?? null) ? trim($_POST['username']) : '';
    $password = is_string($_POST['password'] ?? null) ? $_POST['password'] : '';
    if (attempt_registration_login($username, $password)) {
        clear_old_input();
        redirect('/teachers.php');
    }
    flash('error', 'Usuário ou senha inválidos.');
    redirect('/registro-login.php');
}

render('registration-login', ['pageTitle' => 'Acesso ao cadastro', 'standalone' => true]);
