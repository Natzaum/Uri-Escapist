<section class="panel teacher-registration">
    <span class="eyebrow">URI Escapist</span>
    <h1>Cadastro de professores</h1>
    <p class="muted">Entre com o acesso exclusivo de cadastro.</p>
    <form class="stack-form" action="<?= e(url('/registro-login.php')) ?>" method="post">
        <?= csrf_field() ?>
        <label>
            <span>Usuário</span>
            <input type="text" name="username" autocomplete="username" required autofocus>
        </label>
        <label>
            <span>Senha</span>
            <input type="password" name="password" autocomplete="current-password" required>
        </label>
        <button class="button button-primary" type="submit">Entrar</button>
    </form>
    <p><a href="<?= e(url('/login.php')) ?>">Voltar ao login de professores</a></p>
</section>
