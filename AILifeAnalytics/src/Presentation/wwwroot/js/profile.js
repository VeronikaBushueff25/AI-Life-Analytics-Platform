// ── Profile page ─────────────────────────────────────────────

async function loadProfile() {
    showProfileState('loading');
    document.getElementById('profile-loading-text').textContent =
        'Загружаем ваш профиль...';

    const { ok, data } = await ProfileApi.getLatest();

    if (!ok || !data) {
        showProfileState('empty');
        return;
    }

    renderProfile(data);
    showProfileState('content');
    loadProfileHistory();
}

async function generateProfile() {
    const btn = document.getElementById('generate-profile-btn');
    btn.disabled = true;
    btn.textContent = '⏳ Анализирую...';

    showProfileState('loading');

    // Анимируем текст загрузки
    const messages = [
        'AI анализирует ваши данные...',
        'Вычисляем корреляции...',
        'Определяем паттерны...',
        'Строим поведенческий портрет...',
        'Почти готово...',
    ];
    let i = 0;
    const interval = setInterval(() => {
        const el = document.getElementById('profile-loading-text');
        if (el) el.textContent = messages[i++ % messages.length];
    }, 2000);

    const { ok, data, error } = await ProfileApi.generate();

    clearInterval(interval);
    btn.disabled = false;
    btn.textContent = '✦ Построить профиль';

    if (!ok) {
        showProfileState('empty');
        toast(error || 'Ошибка генерации профиля', 'error');
        return;
    }

    renderProfile(data);
    showProfileState('content');
    toast('Профиль построен! 🎉', 'success');
    loadProfileHistory();
}

function renderProfile(p) {
    // Архетип
    document.getElementById('archetype-emoji').textContent = p.archetypeEmoji || '🧬';
    document.getElementById('archetype-name').textContent = p.archetypeName;
    document.getElementById('archetype-desc').textContent = p.archetypeDescription;

    const from = new Date(p.periodFrom).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' });
    const to = new Date(p.periodTo).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' });
    document.getElementById('archetype-period').textContent =
        `Анализ за ${p.daysAnalyzed} дней · ${from} — ${to}`;

    // Паттерны
    document.getElementById('peak-pattern').textContent = p.peakPerformancePattern;
    document.getElementById('energy-pattern').textContent = p.energyPattern;
    document.getElementById('stress-pattern').textContent = p.stressPattern;

    // Суперсилы
    document.getElementById('superpowers-list').innerHTML =
        (p.superpowers || []).map(s =>
            `<li class="profile-list-item superpower-item">
         <span class="list-bullet">✦</span>${escapeHtml(s)}
       </li>`
        ).join('');

    // Уязвимости
    document.getElementById('vulnerabilities-list').innerHTML =
        (p.vulnerabilities || []).map(v =>
            `<li class="profile-list-item vulnerability-item">
         <span class="list-bullet">△</span>${escapeHtml(v)}
       </li>`
        ).join('');

    // Оптимальные условия
    document.getElementById('opt-sleep').textContent = `${p.optimalSleepHours}ч`;
    document.getElementById('opt-work').textContent = `${p.optimalWorkHours}ч`;
    document.getElementById('opt-day').textContent = p.mostProductiveDayOfWeek;

    // Корреляции с цветом и описанием
    renderCorrelation('corr-sleep', p.correlationSleepFocus);
    renderCorrelation('corr-stress', p.correlationStressMood);

    // Рекомендации
    document.getElementById('recommendations-list').innerHTML =
        (p.recommendations || []).map((r, i) =>
            `<li class="recommendation-item">
         <span class="rec-number">${i + 1}</span>
         <span>${escapeHtml(r)}</span>
       </li>`
        ).join('');

    // Прогноз
    document.getElementById('forecast-text').textContent = p.forecastText;
    const riskBadge = document.getElementById('forecast-risk-badge');
    riskBadge.textContent = riskLabel(p.forecastRisk);
    riskBadge.className = `forecast-risk-badge risk-${(p.forecastRisk || 'medium').toLowerCase()}`;

    const forecastCard = document.getElementById('forecast-card');
    forecastCard.className = `profile-forecast-card forecast-${(p.forecastRisk || 'medium').toLowerCase()}`;

    // Полный анализ
    document.getElementById('full-analysis').textContent = p.fullAnalysis;
}

function renderCorrelation(elId, value) {
    const el = document.getElementById(elId);
    if (!el) return;
    const abs = Math.abs(value);
    const dir = value > 0 ? '↑' : '↓';

    let strength, cls;
    if (abs >= 0.7) { strength = 'Сильная'; cls = 'corr-strong'; }
    else if (abs >= 0.4) { strength = 'Средняя'; cls = 'corr-medium'; }
    else { strength = 'Слабая'; cls = 'corr-weak'; }

    el.innerHTML =
        `<span class="${cls}">${dir} ${strength}</span>
     <span class="corr-value">${value > 0 ? '+' : ''}${value.toFixed(2)}</span>`;
}

function riskLabel(risk) {
    return { Low: 'Низкий риск', Medium: 'Средний риск', High: 'Высокий риск' }
    [risk] ?? risk;
}

async function loadProfileHistory() {
    const { ok, data } = await ProfileApi.getHistory(5);
    if (!ok || !data || data.length <= 1) return;

    const section = document.getElementById('profile-history-section');
    const list = document.getElementById('profile-history-list');
    section.style.display = 'block';

    list.innerHTML = data.map(p => `
    <div class="history-profile-card">
      <div class="hp-emoji">${p.archetypeEmoji}</div>
      <div class="hp-info">
        <div class="hp-name">${escapeHtml(p.archetypeName)}</div>
        <div class="hp-date">${new Date(p.generatedAt).toLocaleDateString('ru-RU',
        { day: 'numeric', month: 'long', year: 'numeric' })}
        </div>
        <div class="hp-days">${p.daysAnalyzed} дней анализа</div>
      </div>
      <div class="hp-risk risk-${(p.forecastRisk || 'medium').toLowerCase()}">
        ${riskLabel(p.forecastRisk)}
      </div>
    </div>`
    ).join('');
}

function showProfileState(state) {
    const states = ['empty', 'loading', 'content'];
    states.forEach(s => {
        const el = document.getElementById(`profile-${s}`);
        if (el) el.style.display = s === state ? 'block' : 'none';
    });
}