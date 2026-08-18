<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';

if (current_teacher() !== null) {
    redirect('/index.php');
}

if (is_post()) {
    verify_csrf();

    $email = trim((string) ($_POST['email'] ?? ''));
    $password = (string) ($_POST['password'] ?? '');
    remember_input(['email' => $email]);

    try {
        if (attempt_login($email, $password)) {
            clear_old_input();
            flash('success', 'Bem-vindo ao painel do URI Escapist.');
            redirect('/index.php');
        }

        flash('error', 'E-mail ou senha inválidos.');
    } catch (PDOException) {
        flash('error', 'Não foi possível conectar ao banco. Confira a instalação e o arquivo config/local.php.');
    }

    redirect('/login.php');
}

render('login', [
    'pageTitle' => 'Entrar',
]);
