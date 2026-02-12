// API Configuration
// - Development: Vite proxy forwards /eshop-api to localhost:5000
// - Production: nginx proxy forwards /eshop-api to backend container

import axios from 'axios';
import type { InternalAxiosRequestConfig } from 'axios';

// API path prefix - matches Aspire resource name and proxy config
export const API_PREFIX = '/eshop-api';

// Always use relative path - proxies handle the routing
// Vite proxy in dev, nginx in production
export const API_BASE_URL = '';
export const GUEST_ID_STORAGE_KEY = 'eshop_guest_id';
export const GUEST_ID_HEADER = 'X-Guest-Id';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
});

let accessToken: string | null = null;
let refreshTokenHandler: (() => Promise<string | null>) | null = null;
let refreshTokenPromise: Promise<string | null> | null = null;

const excludedRefreshPaths = new Set([
    `${API_PREFIX}/react-auth/login`,
    `${API_PREFIX}/react-auth/register`,
    `${API_PREFIX}/react-auth/refresh`,
    `${API_PREFIX}/react-auth/logout`,
]);

type RetriableRequestConfig = InternalAxiosRequestConfig & {
    _retry?: boolean;
};

function createGuestId() {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID();
    }

    return `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
}

export function getOrCreateGuestId() {
    if (typeof window === 'undefined') {
        return null;
    }

    const current = localStorage.getItem(GUEST_ID_STORAGE_KEY)?.trim();
    if (current) {
        return current;
    }

    const created = createGuestId();
    localStorage.setItem(GUEST_ID_STORAGE_KEY, created);
    return created;
}

export function clearGuestId() {
    if (typeof window === 'undefined') {
        return;
    }

    localStorage.removeItem(GUEST_ID_STORAGE_KEY);
}

function runRefreshTokenFlow(): Promise<string | null> {
    if (!refreshTokenHandler) {
        return Promise.resolve(null);
    }

    if (!refreshTokenPromise) {
        refreshTokenPromise = refreshTokenHandler().finally(() => {
            refreshTokenPromise = null;
        });
    }

    return refreshTokenPromise;
}

api.interceptors.request.use((config) => {
    if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
        delete config.headers[GUEST_ID_HEADER];
    } else {
        const guestId = getOrCreateGuestId();
        if (guestId) {
            config.headers[GUEST_ID_HEADER] = guestId;
        }
    }

    return config;
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const statusCode = error.response?.status as number | undefined;
        const requestConfig = error.config as RetriableRequestConfig | undefined;

        if (statusCode !== 401 || !requestConfig || requestConfig._retry) {
            return Promise.reject(error);
        }

        const requestUrl = requestConfig.url ?? '';
        if (excludedRefreshPaths.has(requestUrl)) {
            return Promise.reject(error);
        }

        requestConfig._retry = true;
        const refreshedToken = await runRefreshTokenFlow();
        if (!refreshedToken) {
            return Promise.reject(error);
        }

        requestConfig.headers.Authorization = `Bearer ${refreshedToken}`;
        return api(requestConfig);
    },
);

export function setAccessToken(token: string) {
    accessToken = token;
}

export function clearAccessToken() {
    accessToken = null;
}

export function setRefreshTokenHandler(handler: (() => Promise<string | null>) | null) {
    refreshTokenHandler = handler;
}

export default api;

