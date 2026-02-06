// API Configuration
// - Development: Vite proxy forwards /eshop-api to localhost:5000
// - Production: nginx proxy forwards /eshop-api to backend container

import axios from 'axios';

// API path prefix - matches Aspire resource name and proxy config
export const API_PREFIX = '/eshop-api';

// Always use relative path - proxies handle the routing
// Vite proxy in dev, nginx in production
export const API_BASE_URL = '';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
});

export default api;

