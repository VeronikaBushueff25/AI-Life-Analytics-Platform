// Токен: хранение и чтение 
const AuthToken = {
    KEY: 'ai_analytics_token',
    USER_KEY: 'ai_analytics_user',

    get() { return localStorage.getItem(this.KEY); },
    set(token) { localStorage.setItem(this.KEY, token); },
    remove() { localStorage.removeItem(this.KEY); localStorage.removeItem(this.USER_KEY); },
    exists() { return !!localStorage.getItem(this.KEY); },

    getUser() {
        const raw = localStorage.getItem(this.USER_KEY);
        try { return raw ? JSON.parse(raw) : null; } catch { return null; }
    },
    setUser(u) { localStorage.setItem(this.USER_KEY, JSON.stringify(u)); }
};

// Переключение вкладок Login / Register 
function switchAuthTab(tab) {
    document.getElementById('login-form').style.display = tab === 'login' ? 'flex' : 'none';
    document.getElementById('register-form').style.display = tab === 'register' ? 'flex' : 'none';
    document.getElementById('tab-login').classList.toggle('active', tab === 'login');
    document.getElementById('tab-register').classList.toggle('active', tab === 'register');
    // Сбросить ошибки при переключении
    document.getElementById('login-error').textContent = '';
    document.getElementById('register-error').textContent = '';
}

// Вход 
async function submitLogin(e) {
    e.preventDefault();
    const btn = document.getElementById('login-btn');
    const errorEl = document.getElementById('login-error');
    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value;

    errorEl.textContent = '';
    btn.disabled = true;
    btn.textContent = 'Вхожу...';

    const { ok, data, error } = await apiFetch('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });

    btn.disabled = false;
    btn.textContent = 'Войти';

    if (!ok) {
        errorEl.textContent = error ?? 'Ошибка входа';
        return;
    }

    AuthToken.set(data.token);
    AuthToken.setUser(data.user);
    onAuthSuccess();
}

// Регистрация
async function submitRegister(e) {
    e.preventDefault();
    const btn = document.getElementById('register-btn');
    const errorEl = document.getElementById('register-error');
    const name = document.getElementById('reg-name').value.trim();
    const email = document.getElementById('reg-email').value.trim();
    const password = document.getElementById('reg-password').value;
    const password2 = document.getElementById('reg-password2').value;

    errorEl.textContent = '';

    if (password !== password2) {
        errorEl.textContent = 'Пароли не совпадают';
        return;
    }
    if (password.length < 6) {
        errorEl.textContent = 'Пароль должен быть минимум 6 символов';
        return;
    }

    btn.disabled = true;
    btn.textContent = 'Создаю аккаунт...';

    const { ok, data, error } = await apiFetch('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ email, password, name })
    });

    btn.disabled = false;
    btn.textContent = 'Создать аккаунт';

    if (!ok) {
        errorEl.textContent = error ?? 'Ошибка регистрации';
        return;
    }

    AuthToken.set(data.token);
    AuthToken.setUser(data.user);
    onAuthSuccess();
}

// После успешного входа
function onAuthSuccess() {
    // Показать sidebar, скрыть auth-overlay
    document.getElementById('auth-overlay').style.display = 'none';
    document.querySelector('.sidebar').style.display = '';
    document.getElementById('main-container').style.display = '';

    // Обновить имя пользователя в sidebar
    updateSidebarUser();

    // Перейти на дашборд
    navigate('dashboard');
}

// Выход 
function logout() {
    AuthToken.remove();
    showAuthOverlay();
}

// Обновить имя в sidebar
function updateSidebarUser() {
    const user = AuthToken.getUser();
    const nameEl = document.getElementById('sidebar-user-name');
    const emailEl = document.getElementById('sidebar-user-email');
    const avatarEl = document.getElementById('sidebar-user-avatar');

    if (!user) return;

    if (nameEl) nameEl.textContent = user.name || 'Пользователь';
    if (emailEl) emailEl.textContent = user.email || '';

    // Аватар — первая буква имени
    if (avatarEl) avatarEl.textContent = (user.name || user.email || '?')[0].toUpperCase();
}

// Показать экран авторизации
function showAuthOverlay() {
    document.getElementById('auth-overlay').style.display = 'flex';
    document.querySelector('.sidebar').style.display = 'none';
    document.getElementById('main-container').style.display = 'none';
}

// Проверка токена при старте
async function checkAuth() {
    if (!AuthToken.exists()) {
        showAuthOverlay();
        return false;
    }

    // Проверить что токен ещё валиден
    const { ok } = await apiFetch('/auth/me');
    if (!ok) {
        AuthToken.remove();
        showAuthOverlay();
        return false;
    }

    onAuthSuccess();
    return true;
}