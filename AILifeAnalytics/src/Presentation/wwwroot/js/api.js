const API_BASE = '/api';

async function apiFetch(endpoint, options = {}) {
    try {
        // Автоматически добавляем Authorization если есть токен
        const token = AuthToken?.get?.();
        const authHeader = token ? { 'Authorization': `Bearer ${token}` } : {};

        const res = await fetch(API_BASE + endpoint, {
            headers: {
                'Content-Type': 'application/json',
                ...authHeader,
                ...options.headers
            },
            ...options
        });

        // 401, выйти
        if (res.status === 401) {
            AuthToken?.remove?.();
            showAuthOverlay?.();
            return { ok: false, error: 'Сессия истекла. Войдите снова.', status: 401 };
        }

        const json = await res.json();
        return {
            ok: json.success === true,
            data: json.data,
            error: json.error,
            status: res.status
        };
    } catch (e) {
        return { ok: false, error: 'Ошибка сети. Убедитесь, что сервер запущен.' };
    }
}

const SettingsApi = {
    getProviders: () => apiFetch('/settings/providers'),
    save: (payload) => apiFetch('/settings', { method: 'POST', body: JSON.stringify(payload) }),
    testProxy: (payload) => apiFetch('/settings/test-proxy', { method: 'POST', body: JSON.stringify(payload) }),
};

const ActivityApi = {
    getAll: () => apiFetch('/activity'),
    create: (payload) => apiFetch('/activity', { method: 'POST', body: JSON.stringify(payload) }),
    delete: (id) => apiFetch(`/activity/${id}`, { method: 'DELETE' }),
};

const DashboardApi = {
    get: () => apiFetch('/dashboard'),
};

const AiApi = {
    getInsights: (count = 20) => apiFetch(`/ai/insights?count=${count}`),
    analyze: () => apiFetch('/ai/analyze', { method: 'POST' }),
    analyzePatterns: () => apiFetch('/ai/patterns', { method: 'POST' }),
};

const AuthApi = {
    login: (email, password) => apiFetch('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
    register: (email, password, name) => apiFetch('/auth/register', { method: 'POST', body: JSON.stringify({ email, password, name }) }),
    me: () => apiFetch('/auth/me'),
};

const ProfileApi = {
    generate: () => apiFetch('/profile/generate', { method: 'POST' }),
    getLatest: () => apiFetch('/profile/latest'),
    getHistory: (count = 5) => apiFetch(`/profile/history?count=${count}`),
};

const CbtApi = {
    getAll: (count = 20) => apiFetch(`/cbt?count=${count}`),
    getById: (id) => apiFetch(`/cbt/${id}`),
    create: (payload) => apiFetch('/cbt', {
        method: 'POST', body: JSON.stringify(payload)
    }),
    complete: (id, payload) => apiFetch(`/cbt/${id}/complete`, {
        method: 'PUT', body: JSON.stringify(payload)
    }),
    delete: (id) => apiFetch(`/cbt/${id}`, { method: 'DELETE' }),
    getStats: () => apiFetch('/cbt/stats'),
};

const AchievementsApi = {
    getAll: () => apiFetch('/achievements'),
};