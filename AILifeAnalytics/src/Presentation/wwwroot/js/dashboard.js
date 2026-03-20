let mainChart = null;
let timeChart = null;
let chartData = {};

async function loadDashboard() {
    document.getElementById('dashboard-date').textContent =
        new Date().toLocaleDateString('ru-RU', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

    const { ok, data, error } = await DashboardApi.get();
    if (!ok) {
        toast(error || 'Не удалось загрузить дашборд', 'error');
        renderEmptyKPIs();
        return;
    }

    chartData = data;
    renderKPIs(data.metrics);
    renderMainChart(data.productivityChart || [], 'productivity');
    renderTimeChart(data.timeDistribution);
    loadLatestInsight();
}

function renderEmptyKPIs() {
    const grid = document.getElementById('kpi-grid');
    grid.innerHTML = `
        <div class="kpi-card" style="--kpi-color:var(--text3)">
            <span class="kpi-icon">📭</span>
            <div class="kpi-label">Нет данных</div>
            <div class="kpi-value" style="font-size:18px;color:var(--text2)">Добавьте первую запись</div>
        </div>`;
}

function renderKPIs(m) {
    const grid = document.getElementById('kpi-grid');

    const burnoutColor = m.burnoutStatus === 'Low' ? 'var(--green)'
        : m.burnoutStatus === 'Medium' ? 'var(--amber)' : 'var(--red)';

    const badgeClass = `badge-${m.burnoutStatus.toLowerCase()}`;

    grid.innerHTML = `
        <div class="kpi-card" style="--kpi-color:var(--accent)">
            <span class="kpi-icon">⚡</span>
            <div class="kpi-label">Продуктивность</div>
            <div class="kpi-value">${Math.round(m.productivityScore)}<small style="font-size:16px;color:var(--text2)">/100</small></div>
            <span class="kpi-badge badge-streak">${scoreLabel(m.productivityScore)}</span>
        </div>

        <div class="kpi-card" style="--kpi-color:var(--green)">
            <span class="kpi-icon">🔋</span>
            <div class="kpi-label">Энергия</div>
            <div class="kpi-value">${Math.round(m.energyLevel)}<small style="font-size:16px;color:var(--text2)">/100</small></div>
            <span class="kpi-badge ${m.energyLevel > 60 ? 'badge-low' : m.energyLevel > 35 ? 'badge-medium' : 'badge-high'}">${scoreLabel(m.energyLevel)}</span>
        </div>

        <div class="kpi-card" style="--kpi-color:${burnoutColor}">
            <span class="kpi-icon">🔥</span>
            <div class="kpi-label">Риск выгорания</div>
            <div class="kpi-value" style="color:${burnoutColor}">${Math.round(m.burnoutRisk)}<small style="font-size:16px;color:var(--text2)">/100</small></div>
            <span class="kpi-badge ${badgeClass}">${m.burnoutStatus}</span>
        </div>

        <div class="kpi-card" style="--kpi-color:#ff6b9d">
            <span class="kpi-icon">☯</span>
            <div class="kpi-label">Баланс жизни</div>
            <div class="kpi-value">${Math.round(m.lifeBalanceIndex)}<small style="font-size:16px;color:var(--text2)">/100</small></div>
            <span class="kpi-badge badge-streak">${scoreLabel(m.lifeBalanceIndex)}</span>
        </div>

        <div class="kpi-card" style="--kpi-color:var(--amber)">
            <span class="kpi-icon">🔥</span>
            <div class="kpi-label">Стрик</div>
            <div class="kpi-value">${m.consistencyStreak}<small style="font-size:16px;color:var(--text2)"> д.</small></div>
            <span class="kpi-badge badge-streak">Подряд</span>
        </div>`;
}

// ── Charts ───────────────────────────────────────────────────────────────────

function renderMainChart(points, type) {
    if (mainChart) mainChart.destroy();

    const dark = document.body.classList.contains('dark');
    const gridColor = dark ? 'rgba(255,255,255,0.06)' : 'rgba(0,0,0,0.06)';
    const textColor = dark ? '#8a8a9a' : '#666';

    const colors = {
        productivity: { line: '#7c6fff', fill: 'rgba(124,111,255,0.1)' },
        mood: { line: '#ff6b9d', fill: 'rgba(255,107,157,0.1)' },
        sleep: { line: '#4fffb0', fill: 'rgba(79,255,176,0.1)' }
    };
    const c = colors[type] || colors.productivity;

    const labels = points.map(p => p.label);
    const values = points.map(p => Math.round(p.value * 10) / 10);

    mainChart = new Chart(document.getElementById('main-chart'), {
        type: 'line',
        data: {
            labels,
            datasets: [{
                data: values,
                borderColor: c.line,
                backgroundColor: c.fill,
                borderWidth: 2,
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointBackgroundColor: c.line,
                pointBorderColor: '#fff',
                pointBorderWidth: 1.5,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: dark ? '#111118' : '#fff',
                    borderColor: dark ? 'rgba(255,255,255,0.1)' : 'rgba(0,0,0,0.1)',
                    borderWidth: 1,
                    titleColor: textColor,
                    bodyColor: c.line,
                    padding: 10,
                    callbacks: { label: ctx => ` ${ctx.parsed.y}` }
                }
            },
            scales: {
                x: {
                    grid: { color: gridColor, drawBorder: false },
                    ticks: { color: textColor, font: { size: 11 }, maxTicksLimit: 10, autoSkip: true }
                },
                y: {
                    grid: { color: gridColor, drawBorder: false },
                    ticks: { color: textColor, font: { size: 11 } },
                    min: 0
                }
            }
        }
    });
}

function switchChart(type) {
    document.querySelectorAll('.chart-tab').forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');

    let points;
    if (type === 'mood') points = chartData.moodChart || [];
    else if (type === 'sleep') points = chartData.sleepChart || [];
    else points = chartData.productivityChart || [];

    renderMainChart(points, type);
}

function renderTimeChart(td) {
    if (timeChart) timeChart.destroy();
    if (!td) return;

    const sleep = td.avgSleepHours;
    const work = td.avgWorkHours;
    const leisure = td.avgLeisureHours;
    if (sleep + work + leisure === 0) return;

    timeChart = new Chart(document.getElementById('time-chart'), {
        type: 'doughnut',
        data: {
            labels: ['Сон', 'Работа', 'Отдых'],
            datasets: [{
                data: [sleep, work, leisure],
                backgroundColor: ['#7c6fff', '#ff6b9d', '#4fffb0'],
                borderWidth: 0,
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            plugins: { legend: { display: false } }
        }
    });

    const items = [
        { label: 'Сон', val: `${sleep.toFixed(1)}ч`, color: '#7c6fff' },
        { label: 'Работа', val: `${work.toFixed(1)}ч`, color: '#ff6b9d' },
        { label: 'Отдых', val: `${leisure.toFixed(1)}ч`, color: '#4fffb0' }
    ];
    document.getElementById('time-legend').innerHTML = items.map(i => `
        <div class="time-legend-item">
            <span class="time-legend-label">
                <span class="time-legend-dot" style="background:${i.color}"></span>${i.label}
            </span>
            <span class="time-legend-val">${i.val}</span>
        </div>`).join('');
}

function toggleTheme() {
    const isDark = document.body.classList.toggle('dark');
    document.body.classList.toggle('light', !isDark);
    document.getElementById('theme-icon').textContent = isDark ? '☀' : '☾';
    if (chartData.productivityChart) {
        renderMainChart(chartData.productivityChart, 'productivity');
        renderTimeChart(chartData.timeDistribution);
    }
}