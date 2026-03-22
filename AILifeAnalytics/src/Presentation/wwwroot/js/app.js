
let currentPage = 'dashboard';
const pageCache = {};

const pageInitializers = {
    dashboard: () => loadDashboard(),
    settings: () => loadSettings(),
    insights: () => loadInsights(),
    history: () => loadHistory(),
    entry: () => setupEntryForm(),
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

function toggleTheme() {
    const isDark = document.body.classList.toggle('dark');
    document.body.classList.toggle('light', !isDark);

    const icon = document.getElementById('theme-icon');
    if (icon) {
        icon.textContent = isDark ? '☀' : '☾';
    }

    const productivityCanvas = document.getElementById('productivityChart');
    const timeCanvas = document.getElementById('timeChart');

    if (chartData.productivityChart && productivityCanvas) {
        renderMainChart(chartData.productivityChart, 'productivity');
    }

    if (chartData.timeDistribution && timeCanvas) {
        renderTimeChart(chartData.timeDistribution);
    }
}

// Init

document.addEventListener('DOMContentLoaded', async () => {
    const res = await fetch('/pages/login.html');
    const html = await res.text();
    document.getElementById('auth-overlay').innerHTML = html;

    await checkAuth();
});