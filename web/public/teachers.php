<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/bootstrap.php';
require_once WEB_ROOT . '/src/registration_auth.php';
require_registration_admin();
require_once WEB_ROOT . '/src/teacher_registration.php';

if (is_post()) {
    verify_csrf();
    $name = is_string($_POST['name'] ?? null) ? trim($_POST['name']) : '';
    $email = is_string($_POST['email'] ?? null) ? trim($_POST['email']) : '';
    $password = is_string($_POST['password'] ?? null) ? $_POST['password'] : '';
    remember_input(['name' => $name, 'email' => $email]);

    try {
        register_teacher($name, $email, $password);
        clear_old_input();
        flash('success', 'Professor cadastrado! A conta já pode acessar o painel.');
    } catch (InvalidArgumentException $exception) {
        flash('error', $exception->getMessage());
    } catch (PDOException $exception) {
        flash('error', (string) $exception->getCode() === '23000'
            ? 'Já existe uma conta com este e-mail.'
            : 'Não foi possível cadastrar o professor. Tente novamente.');
    }
    redirect('/teachers.php');
}

render('teachers', ['pageTitle' => 'Professores', 'standalone' => true]);
