async function loadLatestInsight() {
    const { ok, data } = await AiApi.getInsights(1);
    if (ok && data && data.length > 0) {
        renderInsightInCard(data[0].content);
    }
}

async function generateInsight() {
    const btn = document.getElementById('ai-btn');
    const content = document.getElementById('ai-content');
    btn.disabled = true;
    content.innerHTML = '<div class="ai-loading"><div class="ai-spinner"></div> Анализирую данные...</div>';

    const { ok, data, error } = await AiApi.analyze();
    btn.disabled = false;

    if (!ok) {
        content.innerHTML = `<p style="color:var(--red)">${error || 'Ошибка AI-анализа'}</p>`;
        return;
    }
    renderInsightInCard(data.content);
    toast('AI-анализ готов!', 'success');
}

function renderInsightInCard(text) {
    document.getElementById('ai-content').innerHTML = `<p>${escapeHtml(text)}</p>`;
}

async function generatePatterns() {
    toast('Анализирую паттерны...', 'info');
    const { ok, error } = await AiApi.analyzePatterns();
    if (!ok) { toast(error || 'Ошибка', 'error'); return; }
    toast('Анализ паттернов готов!', 'success');
    loadInsights();
}

async function loadInsights() {
    const list = document.getElementById('insights-list');
    list.innerHTML = '<div class="loading-state">Загрузка инсайтов...</div>';

    const { ok, data, error } = await AiApi.getInsights(20);
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