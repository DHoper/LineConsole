# LineConsole

LineConsole 是一個讓店家串接並管理自己 LINE 官方帳號的後台：圖文選單（含排程）、好友、推播排程等。

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

---

## 技術架構

| 層級 | 技術 |
|------|------|
| 後端 | ASP.NET Core（.NET 10），Clean Architecture 分層：Domain、Application、Infrastructure、Server |
| 前端 | React 19＋TypeScript＋Vite＋Tailwind／shadcn/ui |
| 資料庫 | SQL Server＋EF Core（展示模式用 SQLite） |
| 認證 | ASP.NET Identity＋JWT |
| 外部整合 | LINE Messaging API |
| 測試、CI | xUnit v3、GitHub Actions |

---

## 開發

| 做什麼 | 指令 |
|---|---|
| 前端熱更新 | `dotnet run --project LineConsole.Server --launch-profile dev`（會一起開 Vite） |
| 測試 | `dotnet test --solution LineConsole.sln` |
| 新增 migration | `dotnet tool restore`，再 `dotnet ef migrations add <名稱> -p LineConsole.Infrastructure -s LineConsole.Server -o Data/Migrations` |

變更紀錄見 [CHANGELOG.md](CHANGELOG.md)。
