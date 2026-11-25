# LineConsole

LineConsole 是一個讓店家串接並管理自己 LINE 官方帳號的後台：圖文選單（含排程）、好友（LINE Webhook）、概覽儀表板等。

> 改版中（`revamp` 分支）。目前可以一行指令啟動「展示模式」，不需要 LINE 帳號。完整文件會在改版最後補齊。

---

## 快速開始

不需要 LINE 帳號，也不需要資料庫。

**方法一：.NET＋Node（.NET 10 SDK、Node 20 以上）**

```
git clone https://github.com/DHoper/LineConsole.git
cd LineConsole
dotnet run --project LineConsole.Server
```

- 第一次執行會自動建置前端，大約一分鐘。
- 打開 http://localhost:5294

**方法二：Docker**

```
docker compose up --build
```

- 打開 http://localhost:8080

**展示帳號**：`demo@lineconsole.dev`／`Demo1234`（登入頁可以一鍵帶入）

- 展示模式的資料都是假的，LINE 也是模擬的。
- 可以隨意操作，重新啟動就會還原。
- API 文件：http://localhost:5294/swagger

---

## 功能

| 功能 | 狀態 |
|------|------|
| 註冊、登入（JWT） | 完成 |
| LINE 官方帳號管理：新增時向 LINE 驗證 token、憑證加密存放、多帳號切換 | 完成 |
| 圖文選單：建立（含分區編輯）、預覽、設為預設、刪除 | 完成 |
| 圖文選單排程：指定時段自動上線，結束後自動還原原本的預設選單 | 完成 |
| LINE Webhook：簽章驗證（X-Line-Signature）、重送去重、先存收件匣再由背景服務處理 | 完成 |
| 好友管理：加入、封鎖、最後互動時間；搜尋、篩選、分頁 | 完成 |
| 概覽儀表板：好友人數 30 天趨勢、每日新增與封鎖、排程狀態 | 完成 |
| 推播排程 | 規劃中 |
| 標籤、對話、聊天機器人、集點卡、抽獎等 | 規劃中 |

展示模式可以看的兩個動態：

- 啟動 2 分鐘後，咖啡館的「週年慶活動選單」會依排程自動變成預設選單，10 分鐘後自動還原。開著圖文選單頁就看得到。
- 概覽頁的「模擬 LINE Webhook」：由系統扮演 LINE 送出事件（新好友、封鎖、傳訊息），走的是正式的驗簽、去重流程。
  - 「LINE 重送上一個事件」會被去重擋下。
  - 「送一個偽造簽章的請求」會被拒絕（401）。

---

## 執行模式

| 模式 | 資料庫 | LINE | 用途 |
|------|--------|------|------|
| `Demo`（預設） | SQLite 記憶體資料庫，啟動時放入展示資料 | 模擬（`FakeLineClient`） | 展示、開發 |
| `Live` | SQL Server，啟動時自動套用 migration | 真的 LINE Messaging API | 正式使用 |

正式模式要設定以下三項（用環境變數或 `dotnet user-secrets`，不要寫進檔案）：

- `LineConsole__Mode=Live`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`（至少 32 字元）

用 Docker 的話，複製 `.env.example` 成 `.env` 填好，再執行 `docker compose --profile live up --build`。

**Webhook**：在 LINE Developers 把 Webhook URL 設成 `https://你的網域/api/webhooks/line/{官方帳號 ID}`，並開啟「Use webhook」。完整網址在概覽頁可以直接複製。

---

## 技術架構

| 層級 | 技術 |
|------|------|
| 後端 | ASP.NET Core（.NET 10），Clean Architecture 分層：Domain、Application、Infrastructure、Server |
| 前端 | React 19＋TypeScript＋Vite＋Tailwind／shadcn/ui |
| 資料庫 | SQL Server＋EF Core（展示模式用 SQLite） |
| 認證 | ASP.NET Identity＋JWT |
| 外部整合 | LINE Messaging API |
| 測試、CI | xUnit v3（單元、整合）、Playwright（端對端）、GitHub Actions |

---

## 開發

| 做什麼 | 指令 |
|---|---|
| 前端熱更新 | `dotnet run --project LineConsole.Server --launch-profile dev`（會一起開 Vite） |
| 測試 | `dotnet test --solution LineConsole.sln` |
| 端對端測試 | 在 `lineconsole.client` 執行 `npm run build`，再執行 `npx playwright test`。會自動用展示模式啟動後端；本機用已安裝的 Edge，不另外下載瀏覽器 |
| 新增 migration | `dotnet tool restore`，再 `dotnet ef migrations add <名稱> -p LineConsole.Infrastructure -s LineConsole.Server -o Data/Migrations` |

變更紀錄見 [CHANGELOG.md](CHANGELOG.md)。
