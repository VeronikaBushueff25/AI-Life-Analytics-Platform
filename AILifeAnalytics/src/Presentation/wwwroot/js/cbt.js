let cbtCurrentRecordId = null;
let cbtSelectedEmotion = 'Anxiety';
let cbtAllRecords = [];

const EMOTIONS = [
    { key: 'Anxiety', label: 'Тревога', emoji: '😰' },
    { key: 'Sadness', label: 'Грусть', emoji: '😢' },
    { key: 'Anger', label: 'Злость', emoji: '😠' },
    { key: 'Shame', label: 'Стыд', emoji: '😳' },
    { key: 'Guilt', label: 'Вина', emoji: '😔' },
    { key: 'Fear', label: 'Страх', emoji: '😨' },
    { key: 'Loneliness', label: 'Одиночество', emoji: '🥺' },
    { key: 'Frustration', label: 'Разочарование', emoji: '😤' },
    { key: 'Other', label: 'Другое', emoji: '💭' },
];

const DISTORTION_DESCRIPTIONS = {
    'Катастрофизация': 'Преувеличение негативных последствий',
    'Чёрно-белое мышление': 'Только крайности, без оттенков',
    'Чтение мыслей': 'Уверенность в том, что думают другие',
    'Предсказание будущего': 'Негативные прогнозы как факт',
    'Сверхобобщение': 'Один случай = всегда и везде',
    'Навешивание ярлыков': 'Жёсткие оценки себя или других',
    'Долженствование': 'Жёсткие правила "должен/обязан"',
    'Персонализация': 'Принятие ответственности за чужое',
    'Эмоциональные рассуждения': 'Чувства как доказательство фактов',
    'Ментальный фильтр': 'Фокус только на негативе',
};

async function loadCbt() {
    buildEmotionGrid();
    await Promise.all([loadCbtStats(), loadCbtList()]);
}

async function loadCbtStats() {
    const { ok, data } = await CbtApi.getStats();
    if (!ok || !data) return;

    const row = document.getElementById('cbt-stats-row');
    if (!row) return;

    row.innerHTML = `
    <div class="cbt-stat-card">
      <div class="cbt-stat-value">${data.totalSessions}</div>
      <div class="cbt-stat-label">Всего сессий</div>
    </div>
    <div class="cbt-stat-card">
      <div class="cbt-stat-value">${data.completedSessions}</div>
      <div class="cbt-stat-label">Завершено</div>
    </div>
    <div class="cbt-stat-card">
      <div class="cbt-stat-value ${data.avgEmotionShift > 0 ? 'positive' : ''}">
        ${data.avgEmotionShift > 0 ? '↓' : ''}${Math.abs(data.avgEmotionShift)}%
      </div>
      <div class="cbt-stat-label">Сдвиг эмоции</div>
    </div>
    ${data.topDistortion && data.topDistortionCount > 0 ? `
    <div class="cbt-stat-card wide">
      <div class="cbt-stat-value small">${data.topDistortion}</div>
      <div class="cbt-stat-label">Частое искажение (${data.topDistortionCount}×)</div>
    </div>` : ''}`;
}

async function loadCbtList(filter = 'all') {
    const { ok, data } = await CbtApi.getAll(50);
    if (!ok) return;

    cbtAllRecords = data || [];
    renderCbtList(filter);
}

function renderCbtList(filter = 'all') {
    const list = document.getElementById('cbt-list');
    if (!list) return;

    let records = cbtAllRecords;
    if (filter === 'completed') records = records.filter(r => r.isCompleted);
    if (filter === 'draft') records = records.filter(r => !r.isCompleted);

    if (!records.length) {
        list.innerHTML = `
      <div class="cbt-empty">
        <div class="cbt-empty-icon">🧠</div>
        <p>Нет записей. Начните первую КПТ-сессию!</p>
      </div>`;
        return;
    }

    list.innerHTML = records.map(r => `
    <div class="cbt-record-card ${r.isCompleted ? 'completed' : 'draft'}"
         onclick="openCbtRecord('${r.id}')">
      <div class="cbt-record-header">
        <div class="cbt-emotion-badge">
          ${getEmotionEmoji(r.primaryEmotion)} ${getEmotionLabel(r.primaryEmotion)}
        </div>
        <div class="cbt-record-date">
          ${new Date(r.createdAt).toLocaleDateString('ru-RU',
        { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })}
        </div>
        <span class="cbt-status-badge ${r.isCompleted ? 'done' : 'in-progress'}">
          ${r.isCompleted ? '✓ Завершено' : '● В процессе'}
        </span>
      </div>

      <p class="cbt-thought-preview">"${escapeHtml(r.automaticThought)}"</p>

      ${r.detectedDistortions?.length ? `
        <div class="cbt-distortions-preview">
          ${r.detectedDistortions.slice(0, 3).map(d =>
            `<span class="distortion-tag">${d}</span>`).join('')}
        </div>` : ''}

      ${r.isCompleted ? `
        <div class="cbt-shift-row">
          <span class="shift-label">Снижение эмоции:</span>
          <span class="shift-value ${r.emotionShift > 0 ? 'positive' : ''}">
            ${r.emotionShift > 0 ? '↓ ' + r.emotionShift + '%' : 'без изменений'}
          </span>
        </div>` : ''}

      <div class="cbt-record-actions" onclick="event.stopPropagation()">
        ${!r.isCompleted ? `
          <button class="btn btn-ghost btn-sm"
                  onclick="continueSession('${r.id}')">Продолжить →</button>` : ''}
        <button class="btn btn-ghost btn-sm danger"
                onclick="deleteCbtRecord('${r.id}')">Удалить</button>
      </div>
    </div>`).join('');
}

function filterCbt(filter, el) {
    document.querySelectorAll('.cbt-filter').forEach(b => b.classList.remove('active'));
    el.classList.add('active');
    renderCbtList(filter);
}

function buildEmotionGrid() {
    const grid = document.getElementById('emotion-grid');
    if (!grid) return;
    grid.innerHTML = EMOTIONS.map(e => `
    <button class="emotion-btn ${e.key === cbtSelectedEmotion ? 'selected' : ''}"
            onclick="selectEmotion('${e.key}', this)">
      <span class="emotion-btn-emoji">${e.emoji}</span>
      <span class="emotion-btn-label">${e.label}</span>
    </button>`).join('');
}

function selectEmotion(key, el) {
    cbtSelectedEmotion = key;
    document.querySelectorAll('.emotion-btn').forEach(b => b.classList.remove('selected'));
    el.classList.add('selected');
}

function updateEmotionIntensity(val) {
    document.getElementById('cbt-emotion-intensity-val').textContent = val + '%';
    const bar = document.getElementById('emotion-bar');
    if (bar) {
        bar.style.width = val + '%';
        bar.style.background = val > 70 ? 'var(--red)'
            : val > 40 ? 'var(--amber)' : 'var(--green)';
    }
}

function updateNewIntensity(val) {
    document.getElementById('ec-val-new').textContent = val + '%';
    const bar = document.getElementById('ec-bar-new');
    if (bar) bar.style.width = val + '%';
}

function startNewCbt() {
    cbtCurrentRecordId = null;
    ['cbt-situation', 'cbt-thought', 'cbt-behavior', 'cbt-physical',
        'cbt-reframed', 'cbt-insight'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
    document.getElementById('cbt-thought-belief').value = 70;
    document.getElementById('cbt-thought-belief-val').textContent = '70%';
    document.getElementById('cbt-emotion-intensity').value = 60;
    document.getElementById('cbt-emotion-intensity-val').textContent = '60%';
    cbtSelectedEmotion = 'Anxiety';
    buildEmotionGrid();

    document.getElementById('cbt-form-section').style.display = 'block';
    document.getElementById('cbt-form-section').scrollIntoView({ behavior: 'smooth' });
    showCbtStep(1);
}

function closeCbtForm() {
    document.getElementById('cbt-form-section').style.display = 'none';
    loadCbtStats();
    loadCbtList();
}

function showCbtStep(n) {
    [1, 2, 3, 4].forEach(i => {
        const el = document.getElementById(`cbt-step-${i}`);
        if (el) el.style.display = i === n ? 'block' : 'none';
    });
}

async function submitCbtStep1() {
    const situation = document.getElementById('cbt-situation').value.trim();
    const thought = document.getElementById('cbt-thought').value.trim();

    if (!situation || !thought) {
        toast('Заполните ситуацию и автоматическую мысль', 'error');
        return;
    }

    const btn = document.getElementById('cbt-submit-1');
    btn.disabled = true;
    btn.textContent = '⏳ AI анализирует...';

    const payload = {
        situation: situation,
        physicalState: document.getElementById('cbt-physical').value.trim(),
        automaticThought: thought,
        thoughtBelief: parseInt(document.getElementById('cbt-thought-belief').value),
        primaryEmotion: cbtSelectedEmotion,
        emotionIntensity: parseInt(document.getElementById('cbt-emotion-intensity').value),
        behavior: document.getElementById('cbt-behavior').value.trim(),
    };

    const { ok, data, error } = await CbtApi.create(payload);

    btn.disabled = false;
    btn.textContent = 'Проанализировать с AI →';

    if (!ok) { toast(error || 'Ошибка анализа', 'error'); return; }

    cbtCurrentRecordId = data.id;
    renderAiAnalysis(data);
    showCbtStep(2);

    document.getElementById('ec-val-old').textContent = data.emotionIntensity + '%';
    document.getElementById('ec-bar-old').style.width = data.emotionIntensity + '%';
    document.getElementById('cbt-new-intensity').value = Math.max(0, data.emotionIntensity - 20);
    updateNewIntensity(Math.max(0, data.emotionIntensity - 20));
}

function renderAiAnalysis(data) {
    const distList = document.getElementById('distortions-list');
    distList.innerHTML = (data.detectedDistortions || []).map(d => `
    <div class="distortion-item">
      <span class="distortion-name">${d}</span>
      <span class="distortion-desc">${DISTORTION_DESCRIPTIONS[d] || ''}</span>
    </div>`).join('') || '<div class="distortion-item">Явных искажений не обнаружено</div>';

    document.getElementById('ai-challenge-text').textContent = data.aiChallenge;

    document.getElementById('evidence-for-text').textContent = data.evidenceFor;
    document.getElementById('evidence-against-text').textContent = data.evidenceAgainst;

    const qList = document.getElementById('socratic-questions');
    qList.innerHTML = (data.aiQuestions || []).map((q, i) => `
    <div class="question-item">
      <span class="question-number">${i + 1}</span>
      <span class="question-text">${escapeHtml(q)}</span>
    </div>`).join('');
}

async function completeCbt() {
    if (!cbtCurrentRecordId) return;

    const reframed = document.getElementById('cbt-reframed').value.trim();
    if (!reframed) { toast('Сформулируйте новую мысль', 'error'); return; }

    const btn = document.getElementById('cbt-complete-btn');
    btn.disabled = true;
    btn.textContent = '⏳ Сохраняю...';

    const payload = {
        reframedThought: reframed,
        newThoughtBelief: parseInt(document.getElementById('cbt-new-belief').value),
        newEmotionIntensity: parseInt(document.getElementById('cbt-new-intensity').value),
        insight: document.getElementById('cbt-insight').value.trim(),
    };

    const { ok, data, error } = await CbtApi.complete(cbtCurrentRecordId, payload);

    btn.disabled = false;
    btn.textContent = 'Завершить сессию ✓';

    if (!ok) { toast(error || 'Ошибка', 'error'); return; }

    const shift = data.emotionShift;
    document.getElementById('cbt-success-stats').innerHTML = `
    <div class="success-stats-row">
      <div class="success-stat">
        <div class="success-stat-val ${shift > 0 ? 'positive' : ''}"
        >${shift > 0 ? '↓ ' + shift + '%' : '0%'}</div>
        <div class="success-stat-label">Снижение эмоции</div>
      </div>
      <div class="success-stat">
        <div class="success-stat-val">${data.newThoughtBelief}%</div>
        <div class="success-stat-label">Уверенность в новой мысли</div>
      </div>
    </div>`;

    document.getElementById('ai-summary-text').textContent = data.aiSummary;
    showCbtStep(4);
    toast('Сессия завершена! 🎉', 'success');
}

async function openCbtRecord(id) {
    const { ok, data } = await CbtApi.getById(id);
    if (!ok || !data) return;

    if (data.isCompleted) {
        renderCompletedRecord(data);
    } else {
        continueSession(id);
    }
}

function renderCompletedRecord(data) {
    cbtCurrentRecordId = data.id;
    document.getElementById('cbt-form-section').style.display = 'block';
    document.getElementById('cbt-form-section').scrollIntoView({ behavior: 'smooth' });

    const shift = data.emotionShift;
    document.getElementById('cbt-success-stats').innerHTML = `
    <div class="success-stats-row">
      <div class="success-stat">
        <div class="success-stat-val">"${escapeHtml(data.automaticThought)}"</div>
        <div class="success-stat-label">Исходная мысль</div>
      </div>
    </div>
    <div class="success-stats-row">
      <div class="success-stat">
        <div class="success-stat-val">"${escapeHtml(data.reframedThought)}"</div>
        <div class="success-stat-label">Новая мысль</div>
      </div>
    </div>
    <div class="success-stats-row">
      <div class="success-stat">
        <div class="success-stat-val ${shift > 0 ? 'positive' : ''}">
          ${shift > 0 ? '↓ ' + shift + '%' : '0%'}
        </div>
        <div class="success-stat-label">Снижение эмоции</div>
      </div>
      <div class="success-stat">
        <div class="success-stat-val">${data.newThoughtBelief}%</div>
        <div class="success-stat-label">Уверенность в новой мысли</div>
      </div>
    </div>`;

    document.getElementById('ai-summary-text').textContent = data.aiSummary || '';
    showCbtStep(4);
}

async function continueSession(id) {
    const { ok, data } = await CbtApi.getById(id);
    if (!ok || !data) return;

    cbtCurrentRecordId = id;
    document.getElementById('cbt-form-section').style.display = 'block';
    document.getElementById('cbt-form-section').scrollIntoView({ behavior: 'smooth' });

    document.getElementById('cbt-situation').value = data.situation;
    document.getElementById('cbt-thought').value = data.automaticThought;
    document.getElementById('cbt-behavior').value = data.behavior;

    renderAiAnalysis(data);

    document.getElementById('ec-val-old').textContent = data.emotionIntensity + '%';
    document.getElementById('ec-bar-old').style.width = data.emotionIntensity + '%';
    document.getElementById('cbt-new-intensity').value = Math.max(0, data.emotionIntensity - 20);
    updateNewIntensity(Math.max(0, data.emotionIntensity - 20));

    showCbtStep(2);
}

async function deleteCbtRecord(id) {
    if (!confirm('Удалить эту сессию?')) return;
    const { ok } = await CbtApi.delete(id);
    if (ok) {
        toast('Сессия удалена', 'success');
        loadCbtList();
        loadCbtStats();
    }
}

function getEmotionEmoji(key) {
    return EMOTIONS.find(e => e.key === key)?.emoji ?? '💭';
}
function getEmotionLabel(key) {
    return EMOTIONS.find(e => e.key === key)?.label ?? key;
}