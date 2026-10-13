<?php

declare(strict_types=1);
require dirname(__DIR__) . '/src/registration_auth.php';
session_start();

function check(bool $value, string $message): void
{
    if (!$value) throw new RuntimeException($message);
}

check(!can_register_teachers(), 'Guest authorized');
$_SESSION['teacher'] = ['id' => 1, 'name' => 'Professor', 'email' => 'teacher@example.com'];
check(!can_register_teachers(), 'Teacher login must not grant registration access');
check(!attempt_registration_login('nata', 'wrong'), 'Wrong password accepted');
check(!attempt_registration_login('other', '12345678'), 'Wrong username accepted');
check(!can_register_teachers(), 'Failed login authorized');
$oldSession = session_id();
check(attempt_registration_login('nata', '12345678'), 'Valid credentials rejected');
check(session_id() !== $oldSession && can_register_teachers(), 'Session was not renewed');
logout_registration_admin();
check(!can_register_teachers(), 'Logout failed');
check(isset($_SESSION['teacher']), 'Registration logout removed teacher session');
session_destroy();
echo "PASS: separate registration access, credentials, session renewal and logout.\n";
