// Config
const API = '/api';
let mainChart = null;
let timeChart = null;
let chartData = {};
let currentPage = 'dashboard';

// Form state
let formState = { sleep: 7, work: 8, focus: 5, mood: 5 };

// Navigation
function navigate(page) {
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
  document.getElementById(`page-${page}`).classList.add('active');
  document.querySelector(`[data-page="${page}"]`).classList.add('active');
  currentPage = page;

  if (page === 'settings') loadSettings();
  if (page === 'dashboard') loadDashboard();
  if (page === 'insights') loadInsights();
  if (page === 'history') loadHistory();
  if (page === 'entry') setupEntryForm();
}

// API Helpers
async function apiFetch(endpoint, options = {}) {
  try {
    const res = await fetch(API + endpoint, {
      headers: { 'Content-Type': 'application/json', ...options.headers },
      ...options
    });
    const json = await res.json();
    return { ok: res.ok, data: json.data, error: json.error, status: res.status };
  } catch (e) {
    return { ok: false, error: 'Ошибка сети. Убедитесь, что сервер запущен.' };
  }
}

// Toast
function toast(msg, type = 'info') {
  const el = document.getElementById('toast');
  el.textContent = msg;
  el.className = `toast show ${type}`;
  setTimeout(() => el.classList.remove('show'), 3500);
}

// Dashboard
async function loadDashboard() {
  document.getElementById('dashboard-date').textContent =
    new Date().toLocaleDateString('ru-RU', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

  const { ok, data, error } = await apiFetch('/dashboard');
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

function scoreLabel(v) {
  if (v >= 75) return 'Отлично';
  if (v >= 50) return 'Хорошо';
  if (v >= 25) return 'Средне';
  return 'Низко';
}

// Charts
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
          callbacks: {
            label: ctx => ` ${ctx.parsed.y}`
          }
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
  const total = sleep + work + leisure;

  if (total === 0) return;

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

  const legend = document.getElementById('time-legend');
  const items = [
    { label: 'Сон', val: `${sleep.toFixed(1)}ч`, color: '#7c6fff' },
    { label: 'Работа', val: `${work.toFixed(1)}ч`, color: '#ff6b9d' },
    { label: 'Отдых', val: `${leisure.toFixed(1)}ч`, color: '#4fffb0' }
  ];
  legend.innerHTML = items.map(i => `
    <div class="time-legend-item">
      <span class="time-legend-label">
        <span class="time-legend-dot" style="background:${i.color}"></span>${i.label}
      </span>
      <span class="time-legend-val">${i.val}</span>
    </div>`).join('');
}

// AI Insight
async function loadLatestInsight() {
  const { ok, data } = await apiFetch('/ai/insights?count=1');
  if (ok && data && data.length > 0) {
    renderInsightInCard(data[0].content);
  }
}

async function generateInsight() {
  const btn = document.getElementById('ai-btn');
  const content = document.getElementById('ai-content');
  btn.disabled = true;
  content.innerHTML = '<div class="ai-loading"><div class="ai-spinner"></div> Анализирую данные...</div>';

  const { ok, data, error } = await apiFetch('/ai/analyze', { method: 'POST' });
  btn.disabled = false;

  if (!ok) {
    content.innerHTML = `<p style="color:var(--red)">${error || 'Ошибка AI-анализа'}</p>`;
    return;
  }
  renderInsightInCard(data.content);
  toast('AI-анализ готов!', 'success');
}

function renderInsightInCard(text) {
  const content = document.getElementById('ai-content');
  content.innerHTML = `<p>${escapeHtml(text)}</p>`;
}

async function generatePatterns() {
  toast('Анализирую паттерны...', 'info');
  const { ok, data, error } = await apiFetch('/ai/patterns', { method: 'POST' });
  if (!ok) { toast(error || 'Ошибка', 'error'); return; }
  toast('Анализ паттернов готов!', 'success');
  loadInsights();
}

// Insights Page
async function loadInsights() {
  const list = document.getElementById('insights-list');
  list.innerHTML = '<div class="loading-state">Загрузка инсайтов...</div>';

  const { ok, data, error } = await apiFetch('/ai/insights?count=20');
  if (!ok) { list.innerHTML = `<div class="loading-state">${error}</div>`; return; }

  if (!data || data.length === 0) {
    list.innerHTML = '<div class="loading-state">Инсайтов пока нет. Нажмите «AI Анализ» на дашборде.</div>';
    return;
  }

  list.innerHTML = data.map(i => `
    <div class="insight-card">
      <div class="insight-meta">
        <span class="insight-type">${i.analysisType === 'patterns' ? 'Паттерны' : 'Анализ'}</span>
        <span class="insight-date">${formatDate(i.date)}</span>
      </div>
      <p class="insight-text">${escapeHtml(i.content)}</p>
      ${i.productivityScore > 0 ? `
        <div class="insight-scores">
          <span class="insight-score">Продуктивность: <strong>${Math.round(i.productivityScore)}/100</strong></span>
          <span class="insight-score">Риск выгорания: <strong style="color:${i.burnoutRisk > 60 ? 'var(--red)' : 'var(--green)'}">${Math.round(i.burnoutRisk)}/100</strong></span>
        </div>` : ''}
    </div>`).join('');
}

// History
async function loadHistory() {
  const body = document.getElementById('history-body');
  const { ok, data, error } = await apiFetch('/activity');
  if (!ok) { body.innerHTML = `<tr><td colspan="8" class="table-empty">${error}</td></tr>`; return; }

  if (!data || data.length === 0) {
    body.innerHTML = '<tr><td colspan="8" class="table-empty">Записей пока нет.</td></tr>';
    return;
  }

  body.innerHTML = data.map(a => {
    const ps = Math.round(a.productivityScore);
    const scoreClass = ps >= 70 ? 'score-high' : ps >= 40 ? 'score-mid' : 'score-low';
    return `
      <tr>
        <td><strong style="color:var(--text)">${formatDate(a.date, true)}</strong></td>
        <td>${a.sleepHours}ч</td>
        <td>${a.workHours}ч</td>
        <td>${'●'.repeat(a.focusLevel)}${'○'.repeat(10 - a.focusLevel)}</td>
        <td>${'●'.repeat(a.mood)}${'○'.repeat(10 - a.mood)}</td>
        <td><span class="score-pill ${scoreClass}">${ps}</span></td>
        <td><span class="score-pill ${a.energyLevel >= 60 ? 'score-high' : 'score-mid'}">${Math.round(a.energyLevel)}</span></td>
        <td><button class="delete-btn" onclick="deleteActivity('${a.id}')" title="Удалить">✕</button></td>
      </tr>`;
  }).join('');
}

async function deleteActivity(id) {
  if (!confirm('Удалить запись?')) return;
  const { ok } = await apiFetch(`/activity/${id}`, { method: 'DELETE' });
  if (ok) { toast('Запись удалена', 'success'); loadHistory(); }
  else toast('Не удалось удалить', 'error');
}

// Entry Form
function setupEntryForm() {
  // Set today's date
  document.getElementById('f-date').value = new Date().toISOString().split('T')[0];

  // Build rating buttons
  buildRating('focus', 5);
  buildRating('mood', 5);

  updatePreview();
}

function buildRating(type, defaultVal) {
  const container = document.getElementById(`${type}-rating`);
  container.innerHTML = Array.from({ length: 10 }, (_, i) => i + 1).map(n => `
    <button type="button" class="rating-btn ${n === defaultVal ? 'selected' : ''}"
            onclick="selectRating('${type}', ${n})">${n}</button>`).join('');
  formState[type] = defaultVal;
}

function selectRating(type, val) {
  formState[type] = val;
  document.getElementById(`${type}-rating`).querySelectorAll('.rating-btn').forEach((btn, i) => {
    btn.classList.toggle('selected', i + 1 === val);
  });
  document.getElementById(`${type}-label`).textContent = val;
  updatePreview();
}

function updateSlider(type, val) {
  formState[type] = parseFloat(val);
  document.getElementById(`${type}-val`).textContent = `${parseFloat(val).toFixed(1)} ч`;
  updatePreview();
}

function updatePreview() {
  const sleep = formState.sleep;
  const work = formState.work;
  const focus = formState.focus;
  const mood = formState.mood;

  // Mirror backend formula
  const sleepQ = Math.min(sleep / 8, 1) * 100;
  const focusS = focus * 10;
  const moodS = mood * 10;
  const overwork = work > 10 ? (work - 10) * 5 : 0;
  const productivity = Math.min(Math.max((sleepQ * 0.3 + focusS * 0.45 + moodS * 0.25 - overwork), 0), 100);

  const sleepF = sleep < 6 ? sleep / 6 * 0.5 : Math.min(sleep / 8, 1);
  const energy = Math.min(Math.max(sleepF * 60 + (mood / 10) * 40, 0), 100);

  document.getElementById('prev-productivity').textContent = Math.round(productivity);
  document.getElementById('prev-energy').textContent = Math.round(energy);
}

async function submitEntry(e) {
  e.preventDefault();
  const btn = document.getElementById('submit-btn');
  btn.disabled = true;
  btn.textContent = 'Сохранение...';

  const payload = {
    date: document.getElementById('f-date').value,
    sleepHours: formState.sleep,
    workHours: formState.work,
    focusLevel: formState.focus,
    mood: formState.mood,
    notes: document.getElementById('f-notes').value
  };

  const { ok, error } = await apiFetch('/activity', {
    method: 'POST',
    body: JSON.stringify(payload)
  });

  btn.disabled = false;
  btn.textContent = 'Сохранить запись';

  if (ok) {
    toast('Запись сохранена! 🎉', 'success');
    document.getElementById('f-notes').value = '';
    navigate('dashboard');
  } else {
    toast(error || 'Ошибка сохранения', 'error');
  }
}

// Theme
function toggleTheme() {
  const isDark = document.body.classList.toggle('dark');
  document.body.classList.toggle('light', !isDark);
  document.getElementById('theme-icon').textContent = isDark ? '☀' : '☾';
  // Re-render charts with new colors
  if (chartData.productivityChart) {
    renderMainChart(chartData.productivityChart, 'productivity');
    renderTimeChart(chartData.timeDistribution);
  }
}

// Utils
function formatDate(dateStr, short = false) {
  const d = new Date(dateStr);
  if (short) return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
  return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function escapeHtml(str) {
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

// Settings
let settingsState = { activeProvider: 'OpenAI', providers: [], keys: {} };

async function loadSettings() {
    if (currentPage !== 'settings') return;

    const { ok, data } = await apiFetch('/settings/providers');
    if (!ok) { toast('Не удалось загрузить настройки', 'error'); return; }

    settingsState.providers = data;
    settingsState.activeProvider = data.find(p => p.isActive)?.name ?? 'OpenAI';
    settingsState.keys = {};

    renderProviderList();
    renderKeysList();
}

function renderProviderList() {
    const container = document.getElementById('provider-list');
    container.innerHTML = settingsState.providers.map(p => {
        const isActive = p.name === settingsState.activeProvider;
        return `
      <label class="provider-card ${isActive ? 'active' : ''}">
        <input type="radio" name="provider" value="${p.name}"
               ${isActive ? 'checked' : ''}
               onchange="settingsState.activeProvider = this.value; renderProviderList()"/>
        <div class="provider-card-info">
          <div class="provider-card-name">${p.name}</div>
          <div class="provider-card-status ${p.hasKey ? 'has-key' : 'no-key'}">
            ${p.hasKey ? '✓ Ключ настроен' : '✗ Ключ не задан'}
          </div>
        </div>
        ${isActive ? '<span class="provider-active-badge">Активен</span>' : ''}
      </label>`;
    }).join('');
}

function renderKeysList() {
    const container = document.getElementById('keys-list');
    container.innerHTML = settingsState.providers.map(p => `
    <div class="form-group">
      <label>
        ${p.name} API Key
        <span class="key-status-dot ${p.hasKey ? 'set' : 'unset'}">
          ${p.hasKey ? '● настроен' : '○ не задан'}
        </span>
      </label>
      <div class="key-input-wrap">
        <input
          type="password"
          id="key-${p.name}"
          placeholder="${p.hasKey
            ? 'оставьте пустым, чтобы не менять'
            : 'Вставьте ключ...'}"
          oninput="settingsState.keys['${p.name}'] = this.value"
          autocomplete="off"
        />
        <button class="key-toggle-btn" type="button"
                onclick="toggleKeyVisibility('key-${p.name}', this)"
                title="Показать/скрыть">👁</button>
      </div>
      <div class="key-hint">${providerKeyHint(p.name)}</div>
    </div>`).join('');
}

function toggleKeyVisibility(inputId, btn) {
    const input = document.getElementById(inputId);
    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';
    btn.style.opacity = isPassword ? '1' : '0.5';
}

function providerKeyHint(name) {
    const hints = {
        OpenAI: 'platform.openai.com → API Keys → Create new secret key',
        DeepSeek: 'platform.deepseek.com → API Keys → Create API Key',
        HuggingFace: 'huggingface.co → Settings → Access Tokens',
        GoogleAI: 'studio.google.com → API & Services → Create Key'
    };
    return hints[name] ?? 'Получите ключ на официальном сайте провайдера';
}

async function saveSettings() {
    const btn = document.querySelector('#page-settings .btn-primary');
    const statusEl = document.getElementById('settings-status');
    btn.disabled = true;
    btn.textContent = 'Сохранение...';
    statusEl.className = 'settings-status-line';
    statusEl.textContent = '';

    const payload = {
        activeProvider: settingsState.activeProvider,
        apiKeys: settingsState.keys
    };

    const { ok, error } = await apiFetch('/settings', {
        method: 'POST',
        body: JSON.stringify(payload)
    });

    btn.disabled = false;
    btn.textContent = 'Сохранить настройки';

    if (ok) {
        toast(`Сохранено. Активен: ${settingsState.activeProvider}`, 'success');
        statusEl.className = 'settings-status-line saved';
        statusEl.textContent = `✓ Сохранено в ${new Date().toLocaleTimeString('ru-RU')}`;
        settingsState.keys = {};
        await loadSettings();
    } else {
        statusEl.className = 'settings-status-line error';
        statusEl.textContent = `✗ ${error || 'Ошибка сохранения'}`;
        toast(error || 'Ошибка сохранения', 'error');
    }
}

// Init 
document.addEventListener('DOMContentLoaded', () => {
  loadDashboard();
});
