# 變更紀錄

格式參考 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)。改版在 `revamp` 分支進行，每個里程碑一次提交。

## [未發佈] revamp

### M1 一行啟動（展示模式）

**新增**
- 展示模式（預設）：SQLite 記憶體資料庫＋模擬的 LINE（`FakeLineClient`）＋展示資料，不需要任何帳號。每次啟動都還原成初始狀態。
  - 展示帳號 `demo@lineconsole.dev`／`Demo1234`，兩個官方帳號（咖啡館、選物店），五個圖文選單（附圖片、預設選單）。
  - 展示資料走正式流程建立：用 `AccountManager` 註冊、用 `RichMenuService` 建立選單，等於每次啟動都把主要流程跑一遍。
  - `FakeLineClient` 照 LINE 的端點、回應格式、檢查規則（尺寸、聊天列 14 字、區塊不能超出圖片）、錯誤碼運作。
- 正式模式（`LineConsole:Mode=Live`）：SQL Server，啟動時自動套用 migration。
- 一行指令啟動：`dotnet run --project LineConsole.Server`。第一次會自動建置前端（需要 Node）。
  - 前端建置到後端的 `wwwroot`：同一個程式、同一個網址提供網頁和 API。
- Docker：多階段 `Dockerfile`（Node 建前端 → .NET 發佈 → 只含執行環境、非 root），`docker-compose.yml`（預設展示模式；`--profile live` 加 SQL Server）。
- `GET /api/system/info`：前端用來判斷展示模式。
  - 登入頁顯示展示帳號、可以一鍵帶入。
  - 上方有展示模式提示條。
- 上方的官方帳號下拉選單可以切換帳號。圖文選單頁改用目前的帳號，不再手動輸入 token。
- 測試增加到 27 個：模擬 LINE 的行為 6 個、展示模式端到端 6 個（登入、取選單、越權、401、API 404、前端頁面不用登入）。

**修正**
- 前端頁面（含首頁）被「預設要登入」的授權規則擋住，沒登入會拿到 401。現在前端頁面、Swagger 都不用登入。
- 不存在的 `/api/...` 路徑會回前端的 HTML，現在回 404 JSON。
- `UseDefaultFiles` 跟 `UseStaticFiles` 的順序相反。
- 圖文選單卡片：預設選單的徽章沒有文字；顯示寫死的假日期和「類型：未提供」。改成顯示「預設中」、聊天列文字、尺寸。

**調整**
- 依賴注入整理成 `AddApplication()`、`AddInfrastructure()`。展示、正式模式的差別集中在 `AddInfrastructure`：只換資料庫和 LINE 客戶端。
- Server 不再參考前端的 `.esproj`：建置後端不需要 Node，Docker 的 .NET 階段也不用裝 Node。
- 啟動設定：`demo`（預設，直接提供 wwwroot）、`dev`（用 SpaProxy 開 Vite 熱更新）。
- 刪除 Visual Studio 範本產生的 `Dockerfile`、兩份 `CHANGELOG.md`（只是範本說明）。

### M0 地基

**安全**
- 越權：任何登入的人帶別人的官方帳號 ID 就能操作別人的圖文選單。
  - 現在使用者的請求一律經過 `IOfficialAccountAccess`，只拿得到自己的帳號的 token。
  - 不是自己的帳號回 404，不透露帳號存不存在。
- JWT 簽章金鑰原本寫在 `appsettings.json`，而且已經推上公開 repo，視為外洩。
  - 已經從檔案拿掉，改從環境變數 `Jwt__Key` 或 user-secrets 讀。
  - 舊金鑰作廢（沒有改寫 git 歷史）。
- 預設管理員帳密不再寫死在程式裡。
- 拿掉全開的 CORS。

**順手修正的 bug**（盤點時沒有列出）
1. 登入後呼叫 `login()` 的參數錯誤：把整個回應物件當成一個參數傳，應該傳三個。
2. 圖文選單 API 的路徑多一層 `/api`：實際打到 `/api/api/accounts/...`。
3. 後端的錯誤訊息從來沒顯示過：前端讀 `data.message`，後端的格式是 `data.error.message`。
4. 收到 403（權限不足）也會被登出；現在只有 401 才登出。

**調整**
- 全部專案升級 .NET 10（LTS）。.NET 8 已在 2026-11-10 停止支援。
- 套件版本集中管理，拿掉 ASP.NET Core 2.x 時代的舊套件。
- 修正專案參考路徑大小寫（Linux／Docker 會建置失敗）。
- 上傳檔案的請求模型從應用層搬到 Server，應用層不再依賴 ASP.NET。
- 補上漏掉的 migration；加入 dotnet-ef 本機工具。
- 33 個非 UTF-8 檔案轉成 UTF-8，加 `.editorconfig`。
- 前端：
  - react-day-picker 升到 9。
  - 修正 21 個 TypeScript 錯誤。
  - 共用的 HTTP 客戶端不再依賴 auth 功能。
  - Vite 只在開發伺服器時產生 HTTPS 憑證。
- 新增測試專案和 GitHub Actions。

### 盤點報告的更正（2026-11-24）

- 非 UTF-8 的檔案實際是 **33 個**（32 個 Big5、1 個 UTF-16 的 README），不是報告寫的 42 個。之前用 `file` 指令估算，有誤判。
- 「綁定 LINE 官方帳號」不是完全沒做：**註冊時可以一起綁一個**，只是之後沒有新增、管理帳號的功能。

### 已知問題（排在 M2）

- 圖文選單的本地紀錄 ID（自己產生的 GUID）跟 LINE 的 ID（`richmenu-…`）對不上，刪除時本地紀錄刪不掉。
- 找不到圖文選單、沒有預設選單時丟 `InvalidOperationException` 或 LINE 的 404，前端會跳錯誤訊息。
- SQL Server 的 `action_data`、`action_uri` 欄位是 `text`（非 Unicode），存中文會壞。要改成 `nvarchar(max)`。
