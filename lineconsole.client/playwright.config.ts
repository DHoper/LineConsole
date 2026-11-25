import { defineConfig, devices } from '@playwright/test';

const isCI = !!process.env.CI;

/**
 * 端對端測試：只測一條關鍵流程（展示帳號登入 → 建立圖文選單 → 建立排程），確認前後端接起來是通的。
 * - 本機：用已經安裝的 Edge（channel: msedge），不另外下載瀏覽器；後端已經在跑就直接用。
 * - CI：用 Playwright 內建的 Chromium；後端用 CI 前面步驟建好的 Release 版，以展示模式啟動。
 * 前端要先 npm run build（輸出到 LineConsole.Server/wwwroot），由後端一起提供。
 */
export default defineConfig({
    testDir: './e2e',
    timeout: 60_000,
    expect: { timeout: 10_000 },
    fullyParallel: false,
    workers: 1,
    retries: isCI ? 1 : 0,
    reporter: isCI ? [['github'], ['html', { open: 'never' }]] : 'list',
    use: {
        baseURL: 'http://localhost:5294',
        locale: 'zh-TW',
        timezoneId: 'Asia/Taipei',
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure',
    },
    projects: [
        {
            name: isCI ? 'chromium' : 'edge',
            use: isCI ? { ...devices['Desktop Chrome'] } : { ...devices['Desktop Edge'], channel: 'msedge' },
        },
    ],
    webServer: {
        command: isCI
            ? 'dotnet run --project ../LineConsole.Server --no-launch-profile -c Release --no-build'
            : 'dotnet run --project ../LineConsole.Server --no-launch-profile -p:SkipSpaBuild=true',
        url: 'http://localhost:5294/api/system/info',
        reuseExistingServer: !isCI,
        timeout: 180_000,
        env: { LineConsole__Mode: 'Demo', ASPNETCORE_URLS: 'http://localhost:5294' },
    },
});
