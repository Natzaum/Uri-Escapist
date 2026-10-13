<section class="panel create-discipline-card teacher-registration">
    <span class="eyebrow">Nova conta</span>
    <h2>Cadastrar professor</h2>
    <p class="muted">Preencha os dados para liberar o acesso ao painel de perguntas.</p>

    <form class="stack-form" action="<?= e(url('/teachers.php')) ?>" method="post">
        <?= csrf_field() ?>
        <label>
            <span>Nome</span>
            <input type="text" name="name" value="<?= e(old('name')) ?>" maxlength="120" autocomplete="name" required autofocus>
        </label>
        <label>
            <span>E-mail</span>
            <input type="email" name="email" value="<?= e(old('email')) ?>" maxlength="190" autocomplete="email" required>
        </label>
        <label>
            <span>Senha</span>
            <span class="password-field">
                <input id="teacher-password" type="password" name="password" minlength="8" maxlength="72" autocomplete="new-password" aria-describedby="password-help" required>
                <button type="button" data-password-toggle="teacher-password" aria-label="Mostrar senha">Mostrar</button>
            </span>
            <small id="password-help">Use pelo menos 8 caracteres.</small>
        </label>
        <button class="button button-primary" type="submit">Cadastrar professor</button>
    </form>
    <form action="<?= e(url('/actions/registration-logout.php')) ?>" method="post">
        <?= csrf_field() ?>
        <button class="button button-ghost" type="submit">Sair do cadastro</button>
    </form>
</section>
