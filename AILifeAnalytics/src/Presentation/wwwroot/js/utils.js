function toast(msg, type = 'info') {
    const el = document.getElementById('toast');
    el.textContent = msg;
    el.className = `toast show ${type}`;
    setTimeout(() => el.classList.remove('show'), 3500);
}

function formatDate(dateStr, short = false) {
    const d = new Date(dateStr);
    if (short) return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
    return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function escapeHtml(str) {
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function scoreLabel(v) {
    if (v >= 75) return 'Отлично';
    if (v >= 50) return 'Хорошо';
    if (v >= 25) return 'Средне';
    return 'Низко';
}