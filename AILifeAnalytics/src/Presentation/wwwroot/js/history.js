async function loadHistory() {
    const body = document.getElementById('history-body');
    const { ok, data, error } = await ActivityApi.getAll();

    if (!ok) {
        body.innerHTML = `<tr><td colspan="8" class="table-empty">${error}</td></tr>`;
        return;
    }
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
    const { ok } = await ActivityApi.delete(id);
    if (ok) { toast('Запись удалена', 'success'); loadHistory(); }
    else toast('Не удалось удалить', 'error');
}