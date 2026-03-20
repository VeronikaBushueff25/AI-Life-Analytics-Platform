
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
    document.getElementById('theme-icon').textContent = isDark ? '☀' : '☾';
    if (chartData.productivityChart) {
        renderMainChart(chartData.productivityChart, 'productivity');
        renderTimeChart(chartData.timeDistribution);
    }
}

// Init

document.addEventListener('DOMContentLoaded', () => {
    navigate('dashboard');
});