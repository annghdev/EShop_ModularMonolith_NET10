import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 3000,
        // Proxy /eshop-api requests to backend during development
        // Rewrite /eshop-api to /api since backend uses /api routes
        proxy: {
            '/eshop-api': {
                target: 'http://localhost:5000',
                changeOrigin: true,
                secure: false,
                rewrite: (path) => path.replace(/^\/eshop-api/, '/api'),
            }
        }
    }
})
