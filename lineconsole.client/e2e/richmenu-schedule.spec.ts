import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, type Page, test } from '@playwright/test';

// 2500×1686 的選單圖（展示資料用的同一張）
const IMAGE = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../LineConsole.Infrastructure/Demo/Images/cafe-main.jpg');

/** <input type="datetime-local"> 的格式 */
const localInput = (d: Date) => {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

async function loginAsDemo(page: Page) {
    await page.goto('/auth/login');
    await page.getByRole('button', { name: '帶入展示帳號' }).click();
    await page.getByRole('button', { name: '登入', exact: true }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByRole('heading', { name: '概覽' })).toBeVisible();
}

test('展示帳號登入 → 建立圖文選單 → 建立排程並看到狀態', async ({ page }) => {
    const name = `E2E 測試選單 ${Date.now()}`;

    await loginAsDemo(page);

    // 圖文選單頁
    await page.getByRole('navigation').getByRole('link', { name: '圖文選單', exact: true }).click();
    await expect(page).toHaveURL(/\/rich-menu$/);
    await page.getByRole('button', { name: '新增選單' }).click();
    const editor = page.getByRole('dialog', { name: '新增圖文選單' });
    await expect(editor).toBeVisible();

    // 基本資料
    await editor.getByLabel('圖文選單名稱').fill(name);
    await editor.getByLabel('聊天列文字').fill('E2E 選單');

    // 上傳圖片、套用 2×2 版型（劃分 4 個區塊）
    await editor.locator('#richmenu-image-upload').setInputFiles(IMAGE);
    await expect(editor.getByRole('button', { name: '重新上傳圖片' })).toBeVisible();
    await editor.getByRole('button', { name: '套用版型' }).click();
    await page.getByRole('button', { name: /^2欄 2列/ }).click();
    await expect(editor.getByText('區塊動作（4 個）')).toBeVisible();

    // 每個區塊都設定「傳送訊息」
    for (let i = 1; i <= 4; i++) {
        const area = editor.locator('div.rounded-md').filter({ has: page.getByRole('heading', { name: `分割區塊 ${i}`, exact: true }) });
        await area.getByRole('combobox').click();
        await page.getByRole('option', { name: '傳送訊息' }).click();
        await area.getByPlaceholder('請輸入要傳送的訊息').fill(`按鈕 ${i}`);
    }

    // 送出：模擬 LINE 會驗證內容，成功後出現在選單列表
    await editor.getByRole('button', { name: '建立選單' }).click();
    await expect(editor).toBeHidden();
    const card = page.locator('div.group').filter({ has: page.getByRole('img', { name }) });
    await expect(card).toBeVisible();

    // 設定排程。同一個帳號的排程時段不能重疊（展示資料啟動後幾分鐘就有一個週年慶排程），
    // 所以排到幾天後的一小時；天數每秒不同，本機重複跑也不會撞到上一次的排程
    await card.getByRole('button', { name: '更多設定' }).click();
    await page.getByRole('menuitem', { name: '設定排程' }).click();
    const scheduleDialog = page.getByRole('dialog', { name: `排程：${name}` });
    const start = new Date(Date.now() + (2 + (Math.floor(Date.now() / 1000) % 360)) * 86_400_000);
    await scheduleDialog.getByLabel('開始').fill(localInput(start));
    await scheduleDialog.getByLabel('結束').fill(localInput(new Date(start.getTime() + 3_600_000)));
    await scheduleDialog.getByRole('button', { name: '建立排程' }).click();
    await expect(scheduleDialog).toBeHidden();

    // 排程列表出現這一筆，狀態「待執行」
    const row = page.getByRole('row').filter({ hasText: name });
    await expect(row).toBeVisible();
    await expect(row.getByText('待執行')).toBeVisible();
});
