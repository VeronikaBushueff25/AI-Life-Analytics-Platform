let formState = { sleep: 7, work: 8, focus: 5, mood: 5 };

function setupEntryForm() {
    document.getElementById('f-date').value = new Date().toISOString().split('T')[0];
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
    const { sleep, work, focus, mood } = formState;

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

    const { ok, error } = await ActivityApi.create(payload);

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