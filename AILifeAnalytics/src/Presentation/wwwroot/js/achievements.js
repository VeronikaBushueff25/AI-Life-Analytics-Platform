
async function loadAchievements() {
    const container = document.getElementById('main-container');
    renderAchievementsSkeleton();

    const { ok, data, error } = await AchievementsApi.getAll();

    if (!ok) {
        toast(error || 'Не удалось загрузить достижения', 'error');
        return;
    }

    renderAchievementsPage(data);

    const badge = document.getElementById('achievements-badge');
    if (badge) badge.style.display = 'none';
}

function renderAchievementsPage(data) {
    const container = document.getElementById('main-container');
    container.innerHTML = buildAchievementsHTML(data);
}

function buildAchievementsHTML(data) {
    return `
    <div class="page-header">
      <div>
        <h1 class="page-title">Достижения</h1>
        <p class="page-subtitle">
          ${data.unlockedCount} из ${data.totalCount} разблокировано
        </p>
      </div>
    </div>

    <!-- Прогресс-бар -->
    <div class="achievements-progress-card">
      <div class="ap-header">
        <span class="ap-label">Общий прогресс</span>
        <span class="ap-value">${data.progress}%</span>
      </div>
      <div class="ap-bar-wrap">
        <div class="ap-bar" style="width: ${data.progress}%"></div>
      </div>
      <div class="ap-counts">
        <span>${data.unlockedCount} получено</span>
        <span>${data.totalCount - data.unlockedCount} осталось</span>
      </div>
    </div>

    <!-- Новые достижения (если есть) -->
    ${data.unlocked.filter(a => a.isNew).length > 0 ? `
      <div class="achievements-section">
        <h3 class="achievements-section-title">
          <span class="section-dot new-dot"></span>Новые
        </h3>
        <div class="achievements-grid">
          ${data.unlocked.filter(a => a.isNew).map(renderUnlocked).join('')}
        </div>
      </div>` : ''}

    <!-- Разблокированные -->
    ${data.unlocked.filter(a => !a.isNew).length > 0 ? `
      <div class="achievements-section">
        <h3 class="achievements-section-title">
          <span class="section-dot unlocked-dot"></span>Получено
        </h3>
        <div class="achievements-grid">
          ${data.unlocked.filter(a => !a.isNew).map(renderUnlocked).join('')}
        </div>
      </div>` : ''}

    <!-- Заблокированные -->
    ${data.locked.length > 0 ? `
      <div class="achievements-section">
        <h3 class="achievements-section-title">
          <span class="section-dot locked-dot"></span>Впереди
        </h3>
        <div class="achievements-grid">
          ${data.locked.map(renderLocked).join('')}
        </div>
      </div>` : ''}
  `;
}

function renderUnlocked(a) {
    const date = new Date(a.unlockedAt).toLocaleDateString('ru-RU', {
        day: 'numeric', month: 'short', year: 'numeric'
    });
    return `
    <div class="achievement-card unlocked ${a.isNew ? 'is-new' : ''}">
      <div class="ach-emoji">${a.emoji}</div>
      <div class="ach-title">${escapeHtml(a.title)}</div>
      <div class="ach-desc">${escapeHtml(a.description)}</div>
      ${a.context ? `<div class="ach-context">${escapeHtml(a.context)}</div>` : ''}
      <div class="ach-date">${date}</div>
      ${a.isNew ? '<div class="ach-new-badge">НОВОЕ</div>' : ''}
    </div>`;
}

function renderLocked(a) {
    return `
    <div class="achievement-card locked">
      <div class="ach-emoji locked-emoji">${a.emoji}</div>
      <div class="ach-title locked-title">${escapeHtml(a.title)}</div>
      <div class="ach-desc">${escapeHtml(a.description)}</div>
      <div class="ach-lock">🔒</div>
    </div>`;
}

function renderAchievementsSkeleton() {
    const container = document.getElementById('main-container');
    container.innerHTML = `
    <div class="page-header">
      <div>
        <h1 class="page-title">Достижения</h1>
        <p class="page-subtitle">Загрузка...</p>
      </div>
    </div>
    <div class="achievements-grid">
      ${Array(6).fill('<div class="achievement-card skeleton"></div>').join('')}
    </div>`;
}

// Confetti-анимация при разблокировании

function showAchievementToast(achievements) {
    if (!achievements || achievements.length === 0) return;

    achievements.forEach((a, i) => {
        setTimeout(() => {
            showAchievementPopup(a);
        }, i * 1500);
    });
}

function showAchievementPopup(a) {
    document.getElementById('achievement-popup')?.remove();

    const popup = document.createElement('div');
    popup.id = 'achievement-popup';
    popup.className = 'achievement-popup';
    popup.innerHTML = `
    <div class="achievement-popup-inner">
      <div class="popup-label">🏆 Достижение разблокировано!</div>
      <div class="popup-emoji">${a.emoji}</div>
      <div class="popup-title">${escapeHtml(a.title)}</div>
      <div class="popup-desc">${escapeHtml(a.description)}</div>
    </div>`;

    document.body.appendChild(popup);
    launchConfetti();

    setTimeout(() => {
        popup.classList.add('hiding');
        setTimeout(() => popup.remove(), 500);
    }, 4000);
}

function launchConfetti() {
    const colors = ['#7c6fff', '#4fffb0', '#ffb347', '#ff6b9d', '#ff5555'];
    const count = 80;

    for (let i = 0; i < count; i++) {
        const el = document.createElement('div');
        el.className = 'confetti-piece';
        el.style.cssText = `
      left: ${Math.random() * 100}vw;
      background: ${colors[Math.floor(Math.random() * colors.length)]};
      animation-duration: ${1.5 + Math.random() * 2}s;
      animation-delay: ${Math.random() * 0.5}s;
      width: ${6 + Math.random() * 8}px;
      height: ${6 + Math.random() * 8}px;
      border-radius: ${Math.random() > 0.5 ? '50%' : '2px'};
    `;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }
}