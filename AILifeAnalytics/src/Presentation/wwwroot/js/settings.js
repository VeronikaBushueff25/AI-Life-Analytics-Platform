let settingsState = { activeProvider: 'OpenAI', providers: [], keys: {} };

async function loadSettings() {
    if (currentPage !== 'settings') return;

    const { ok, data } = await SettingsApi.getProviders();
    if (!ok) { toast('Не удалось загрузить настройки', 'error'); return; }

    settingsState.providers = data.providers;
    settingsState.activeProvider = data.providers.find(p => p.isActive)?.name ?? 'OpenAI';
    settingsState.keys = {};

    renderProviderList();
    renderKeysList();
    fillProxyFields(data.proxy);
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
        apiKeys: settingsState.keys,
        proxy: getProxyFromForm()
    };

    const { ok, error } = await SettingsApi.save(payload);

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

// ── Providers ─────────────────────────────────────────────────────────────────

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
                    placeholder="${p.hasKey ? 'оставьте пустым, чтобы не менять' : 'Вставьте ключ...'}"
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

// ── Proxy ─────────────────────────────────────────────────────────────────────

function toggleProxyFields(enabled) {
    document.getElementById('proxy-fields').style.display = enabled ? 'block' : 'none';
    document.getElementById('proxy-toggle-label').textContent = enabled ? 'Включён' : 'Выключен';
}

function fillProxyFields(proxy) {
    const enabled = proxy?.enabled ?? false;
    document.getElementById('proxy-enabled').checked = enabled;
    document.getElementById('proxy-host').value = proxy?.host ?? '';
    document.getElementById('proxy-port').value = proxy?.port ?? 1080;
    document.getElementById('proxy-username').value = proxy?.username ?? '';
    document.getElementById('proxy-password').value = '';
    toggleProxyFields(enabled);
}

function getProxyFromForm() {
    return {
        enabled: document.getElementById('proxy-enabled').checked,
        host: document.getElementById('proxy-host').value.trim(),
        port: parseInt(document.getElementById('proxy-port').value) || 1080,
        username: document.getElementById('proxy-username').value.trim(),
        password: document.getElementById('proxy-password').value
    };
}

async function testProxy() {
    const btn = document.getElementById('test-proxy-btn');
    const result = document.getElementById('proxy-test-result');
    const proxyData = getProxyFromForm();

    if (!proxyData.host) {
        result.className = 'proxy-test-result error';
        result.textContent = '✗ Укажите хост прокси';
        return;
    }
    if (!proxyData.port || proxyData.port < 1 || proxyData.port > 65535) {
        result.className = 'proxy-test-result error';
        result.textContent = '✗ Укажите корректный порт (1–65535)';
        return;
    }

    btn.disabled = true;
    btn.textContent = '⟳ Проверяю...';
    result.className = 'proxy-test-result';
    result.textContent = '';

    const saveRes = await SettingsApi.save({
        activeProvider: settingsState.activeProvider,
        apiKeys: {},
        proxy: proxyData
    });

    if (!saveRes.ok) {
        btn.disabled = false;
        btn.textContent = '⟳ Проверить соединение';
        result.className = 'proxy-test-result error';
        result.textContent = `✗ Не удалось сохранить настройки: ${saveRes.error ?? 'неизвестная ошибка'}`;
        return;
    }

    try {
        const { ok, data, error } = await SettingsApi.testProxy();
        const message = ok ? (data ?? error ?? 'Нет ответа') : (error ?? 'Нет ответа');
        const isSuccess = ok && data != null && !data.startsWith('✗');
        result.className = `proxy-test-result ${isSuccess ? 'success' : 'error'}`;
        result.textContent = message;
    } finally {
        btn.disabled = false;
        btn.textContent = '⟳ Проверить соединение';
    }
}