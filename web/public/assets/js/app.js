document.querySelectorAll('[data-alert-close]').forEach((button) => {
    button.addEventListener('click', () => button.closest('[data-alert]')?.remove());
});

document.querySelectorAll('[data-password-toggle]').forEach((button) => {
    button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.passwordToggle);

        if (!input) return;

        const showing = input.type === 'text';
        input.type = showing ? 'password' : 'text';
        button.textContent = showing ? 'Mostrar' : 'Ocultar';
        button.setAttribute('aria-label', showing ? 'Mostrar senha' : 'Ocultar senha');
    });
});

document.querySelectorAll('form[data-confirm]').forEach((form) => {
    form.addEventListener('submit', (event) => {
        if (!window.confirm(form.dataset.confirm)) {
            event.preventDefault();
        }
    });
});

const menuToggle = document.querySelector('[data-menu-toggle]');
const sidebar = document.getElementById('sidebar');

menuToggle?.addEventListener('click', () => sidebar?.classList.toggle('is-open'));

document.addEventListener('click', (event) => {
    if (
        sidebar?.classList.contains('is-open') &&
        !sidebar.contains(event.target) &&
        !menuToggle?.contains(event.target)
    ) {
        sidebar.classList.remove('is-open');
    }
});

const themeToggle = document.querySelector('[data-theme-toggle]');
const themeIcon = themeToggle?.querySelector('.theme-toggle-icon');
const themeLabel = themeToggle?.querySelector('.theme-toggle-label');

function setTheme(theme, save = false) {
    document.documentElement.dataset.theme = theme;
    document.querySelector('meta[name="color-scheme"]')?.setAttribute('content', theme);
    const dark = theme === 'dark';
    if (themeToggle) {
        themeToggle.setAttribute('aria-pressed', String(dark));
        themeToggle.setAttribute('aria-label', `Alternar para tema ${dark ? 'claro' : 'escuro'}`);
    }
    if (themeIcon) themeIcon.textContent = dark ? '☀' : '☾';
    if (themeLabel) themeLabel.textContent = dark ? 'Tema claro' : 'Tema escuro';
    if (save) {
        try { localStorage.setItem('uri-escapist-theme', theme); } catch (_) {}
    }
}

setTheme(document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light');
themeToggle?.addEventListener('click', () => {
    setTheme(document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark', true);
});
