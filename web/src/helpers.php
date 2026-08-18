<?php

declare(strict_types=1);

function config(?string $key = null, mixed $default = null): mixed
{
    $value = $GLOBALS['app_config'] ?? [];

    if ($key === null) {
        return $value;
    }

    foreach (explode('.', $key) as $segment) {
        if (!is_array($value) || !array_key_exists($segment, $value)) {
            return $default;
        }

        $value = $value[$segment];
    }

    return $value;
}

function e(mixed $value): string
{
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

function url(string $path = ''): string
{
    $base = (string) config('app.base_url', '');

    if ($path === '') {
        return $base === '' ? '/' : $base;
    }

    return $base . '/' . ltrim($path, '/');
}

function redirect(string $path): never
{
    header('Location: ' . url($path));
    exit;
}

function csrf_token(): string
{
    if (empty($_SESSION['_csrf'])) {
        $_SESSION['_csrf'] = bin2hex(random_bytes(32));
    }

    return (string) $_SESSION['_csrf'];
}

function csrf_field(): string
{
    return '<input type="hidden" name="_csrf" value="' . e(csrf_token()) . '">';
}

function verify_csrf(): void
{
    $submitted = (string) ($_POST['_csrf'] ?? '');
    $stored = (string) ($_SESSION['_csrf'] ?? '');

    if ($stored === '' || !hash_equals($stored, $submitted)) {
        http_response_code(419);
        exit('Sessão expirada. Atualize a página e tente novamente.');
    }
}

function flash(string $type, string $message): void
{
    $_SESSION['_flash'][] = ['type' => $type, 'message' => $message];
}

function consume_flashes(): array
{
    $flashes = $_SESSION['_flash'] ?? [];
    unset($_SESSION['_flash']);

    return is_array($flashes) ? $flashes : [];
}

function remember_input(array $input): void
{
    $_SESSION['_old'] = $input;
}

function old(string $key, mixed $fallback = ''): mixed
{
    return $_SESSION['_old'][$key] ?? $fallback;
}

function clear_old_input(): void
{
    unset($_SESSION['_old']);
}

function slugify(string $value): string
{
    $value = trim(mb_strtolower($value, 'UTF-8'));
    $value = strtr($value, [
        'á' => 'a', 'à' => 'a', 'â' => 'a', 'ã' => 'a', 'ä' => 'a',
        'é' => 'e', 'è' => 'e', 'ê' => 'e', 'ë' => 'e',
        'í' => 'i', 'ì' => 'i', 'î' => 'i', 'ï' => 'i',
        'ó' => 'o', 'ò' => 'o', 'ô' => 'o', 'õ' => 'o', 'ö' => 'o',
        'ú' => 'u', 'ù' => 'u', 'û' => 'u', 'ü' => 'u',
        'ç' => 'c', 'ñ' => 'n',
    ]);
    $transliterated = iconv('UTF-8', 'ASCII//TRANSLIT//IGNORE', $value);
    $value = $transliterated === false ? $value : $transliterated;
    $value = preg_replace('/[^a-z0-9]+/', '-', $value) ?? '';

    return trim($value, '-');
}

function is_post(): bool
{
    return ($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'POST';
}

function render(string $view, array $data = []): void
{
    extract($data, EXTR_SKIP);
    require WEB_ROOT . '/views/layout/header.php';
    require WEB_ROOT . '/views/' . $view . '.php';
    require WEB_ROOT . '/views/layout/footer.php';
}
