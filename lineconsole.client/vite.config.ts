import { fileURLToPath, URL } from 'node:url';

import tailwindcss from '@tailwindcss/vite';
import plugin from '@vitejs/plugin-react';
import child_process from 'child_process';
import fs from 'fs';
import path from 'path';
import { env } from 'process';
import { defineConfig, type ServerOptions } from 'vite';

/**
 * 開發伺服器（npm run dev）才需要 HTTPS 憑證和 /api 代理。
 * 以前憑證在設定檔載入時就產生，連 npm run build 也會去呼叫 dotnet dev-certs，
 * 在只有 Node 的環境（CI、Docker 的前端建置階段）會失敗。
 */
function devServer(): ServerOptions {
    const baseFolder =
        env.APPDATA !== undefined && env.APPDATA !== ''
            ? `${env.APPDATA}/ASP.NET/https`
            : `${env.HOME}/.aspnet/https`;

    const certificateName = 'lineconsole.client';
    const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
    const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

    if (!fs.existsSync(baseFolder)) {
        fs.mkdirSync(baseFolder, { recursive: true });
    }

    if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
        const result = child_process.spawnSync('dotnet', [
            'dev-certs',
            'https',
            '--export-path',
            certFilePath,
            '--format',
            'Pem',
            '--no-password',
        ], { stdio: 'inherit' });
        if (result.status !== 0) {
            throw new Error('Could not create certificate.');
        }
    }

    const target = env.ASPNETCORE_HTTPS_PORT
        ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
        : env.ASPNETCORE_URLS
            ? env.ASPNETCORE_URLS.split(';')[0]
            : 'https://localhost:7144';

    console.log('Proxy target:', target);

    return {
        proxy: {
            '^/api': {
                target,
                secure: false,
            },
        },
        port: parseInt(env.DEV_SERVER_PORT || '54781'),
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        },
    };
}

// https://vitejs.dev/config/
export default defineConfig(({ command }) => ({
    plugins: [plugin(), tailwindcss()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: command === 'serve' ? devServer() : undefined,
}));
