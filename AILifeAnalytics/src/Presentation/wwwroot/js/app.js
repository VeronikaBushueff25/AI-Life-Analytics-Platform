
let currentPage = 'dashboard';
const pageCache = {};

const pageInitializers = {
    dashboard: () => loadDashboard(),
    settings: () => loadSettings(),
    insights: () => loadInsights(),
    history: () => loadHistory(),
    entry: () => setupEntryForm(),
    profile: () => loadProfile(),
    cbt: () => loadCbt(),
    achievements: () => loadAchievements(),
};

const THEMES = [
    { id: 'theme-dark', label: 'Тёмная' },
    { id: 'theme-light', label: 'Светлая' },
    { id: 'theme-cyber', label: 'Кибер' },
    { id: 'theme-paper', label: 'Бумага' },
    { id: 'theme-sakura', label: 'Сакура' },
    { id: 'theme-arctic', label: 'Арктика' },
];

const THEME_ICONS = {
    'theme-dark': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>`,
    'theme-light': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>`,
    'theme-cyber': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>`,
    'theme-paper': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>`,
    'theme-sakura': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>`,
    'theme-arctic': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>`,
};

async function navigate(page) {
    const container = document.getElementById('main-container');
    if (!container) {
        console.error('main-container не найден в DOM');
        return;
    }

    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    document.querySelector(`[data-page="${page}"]`).classList.add('active');
    currentPage = page;

    if (!pageCache[page]) {
        try {
            const res = await fetch(`/pages/${page}.html`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            pageCache[page] = await res.text();
        } catch (e) {
            toast(`Не удалось загрузить страницу: ${e.message}`, 'error');
            return;
        }
    }

    container.innerHTML = pageCache[page];

    const init = pageInitializers[page];
    if (init) init();
}

// Theme

function applyTheme(themeId) {
    THEMES.forEach(t => document.body.classList.remove(t.id));
    document.body.classList.remove('dark', 'light');
    document.body.classList.add(themeId);
    localStorage.setItem('theme', themeId);

    const icon = document.getElementById('theme-icon');
    if (icon) icon.innerHTML = THEME_ICONS[themeId] || '';

    const productivityCanvas = document.getElementById('productivityChart');
    const timeCanvas = document.getElementById('timeChart');
    if (chartData?.productivityChart && productivityCanvas) renderMainChart(chartData.productivityChart, 'productivity');
    if (chartData?.timeDistribution && timeCanvas) renderTimeChart(chartData.timeDistribution);
}

function toggleTheme() {
    showThemePicker();
}

function showThemePicker() {
    document.getElementById('theme-picker')?.remove();
    const current = localStorage.getItem('theme') || 'theme-dark';

    const picker = document.createElement('div');
    picker.id = 'theme-picker';

    picker.innerHTML = `
        <div class="theme-picker-backdrop" onclick="closeThemePicker()"></div>
        <div class="theme-picker-panel">
            <div class="theme-picker-title">Выбор темы</div>
            <div class="theme-picker-grid">
                ${THEMES.map(t => `
                    <button
                        class="theme-option ${t.id === current ? 'active' : ''}"
                        data-theme="${t.id}"
                        onclick="applyTheme('${t.id}'); closeThemePicker();"
                    >
                        <span class="theme-option-preview theme-preview-${t.id}"></span>
                        <span class="theme-option-icon" data-icon="${t.id}"></span>
                        <span class="theme-option-label">${t.label}</span>
                    </button>
                `).join('')}
            </div>
        </div>
    `;

    document.body.appendChild(picker);

    picker.querySelectorAll('[data-icon]').forEach(el => {
        const svg = THEME_ICONS[el.dataset.icon];
        if (svg) el.innerHTML = svg;
    });

    requestAnimationFrame(() => picker.querySelector('.theme-picker-panel').classList.add('visible'));
}

function closeThemePicker() {
    const picker = document.getElementById('theme-picker');
    if (!picker) return;
    const panel = picker.querySelector('.theme-picker-panel');
    panel.classList.remove('visible');
    setTimeout(() => picker.remove(), 200);
}

// Инициализация
document.addEventListener('DOMContentLoaded', async () => {
    const saved = localStorage.getItem('theme') || 'theme-dark';
    applyTheme(saved);

    const res = await fetch('/pages/login.html');
    const html = await res.text();
    document.getElementById('auth-overlay').innerHTML = html;
    await checkAuth();
    await updateAchievementsBadge();
});

// Показать счётчик новых достижений в sidebar
async function updateAchievementsBadge() {
    const { ok, data } = await AchievementsApi.getAll();
    if (!ok || !data) return;

    const badge = document.getElementById('achievements-badge');
    if (!badge) return;

    if (data.unseenCount > 0) {
        badge.textContent = data.unseenCount;
        badge.style.display = 'block';
    } else {
        badge.style.display = 'none';
    }
}