# 變更紀錄

格式參考 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.1.0/)。改版在 `revamp` 分支進行，每個里程碑一次提交。

## [未發佈] revamp

### M2 官方帳號管理、圖文選單排程

**新增**
- 官方帳號管理（`/api/accounts`、「官方帳號」頁）：
  - 列表、新增、切換、解除綁定。
  - 新增時先拿 Access Token 呼叫 LINE 的 `GET /v2/bot/info`，LINE 接受才存，並記下 `@ID`。
  - 同一個使用者不能重複綁同一個 Channel。
  - 解除綁定只刪本地資料（帳號、選單紀錄、排程），LINE 上的選單不受影響。
- 憑證加密：Channel Secret、Access Token 用 ASP.NET Data Protection 加密後存放，API 回應不含這兩個值（只進不出）。
  - 加密金鑰存在資料庫（`DataProtectionKeys`），容器重開、換機器都還解得開。
  - M0 以前的明碼舊資料照樣讀得到，下次寫入時加密。
- 圖文選單排程（`/api/accounts/{id}/richmenu/schedules`）：
  - 指定時段把選單設成預設，結束後自動還原成「排程前的預設選單」。
  - 規則寫在領域模型 `RichMenuSchedule`：
    - 狀態：待執行、進行中、完成、取消、失敗、錯過。
    - 同一個帳號的時段不能重疊。
    - 結束時如果預設選單已經被手動換掉，就尊重手動的設定，不去動它。
    - 整段時間伺服器都沒執行到（例如停機）的排程直接跳過，不補做。
  - `RichMenuScheduleRunner` 處理到期的排程，時間由呼叫的人傳入，測試可以任意指定時間。
    - 每個排程各自處理、各自存檔，一個失敗不影響其他排程，重複執行也安全。
  - 背景服務 `RichMenuScheduleWorker`（BackgroundService＋PeriodicTimer）定時呼叫執行器。展示模式每 10 秒一次，正式模式每 30 秒一次。
- 前端：
  - 官方帳號頁、新增帳號對話框（展示模式有提示：token 用 invalid 開頭可以看 LINE 拒絕的情況）。
  - 上方帳號清單改成即時從 API 讀。
  - 圖文選單卡片有「設定排程」和排程時段；頁面下方有排程清單，可以取消還沒開始的排程。
  - 每 10 秒重抓一次，排程切換時畫面會跟著變。
- 圖文選單編輯器修到能完整使用：
  - 上傳圖片後自動讀尺寸當選單尺寸，不符合 LINE 規定的尺寸不收。
  - 存檔前檢查名稱、聊天列 14 字、每個區塊都要設定動作。
  - 「編輯」改成「以此為範本建立新選單」：LINE 的圖文選單建立後不能修改。
- 模擬 LINE 補上：
  - `GET /v2/bot/info`。
  - `invalid` 開頭的 token 一律回 401。
  - 區塊動作的類型和必填欄位檢查（LINE 沒有 `none` 這種類型）。
- 展示資料多兩個排程：
  - 咖啡館：啟動 2 分鐘後換成週年慶選單，10 分鐘後還原。展示時開著畫面就看得到。
  - 選物店：下週的限時特賣。
- 測試增加到 48 個：
  - 排程的領域規則 11 個（含 4 個重疊判斷案例）。
  - 排程執行器 6 個：開始、還原、原本沒有預設、手動改過、錯過、LINE 失敗、重複執行。
  - 端到端 2 個：新使用者走完「新增帳號 → 建立選單 → 排程 → 自動切換 → 還原 → 別人看不到 → 解除綁定」，另一個測刪除選單同步刪本地紀錄。
  - 模擬 LINE 2 個。

**修正（M1 已知問題）**
- 本地紀錄現在存 LINE 的選單 ID（`line_rich_menu_id`），刪除選單會同步刪掉本地紀錄和它的排程。
- 找不到選單回 404（不再是 500）；沒有預設選單時回 200＋`null`（不再當成錯誤）。
- `action_data`、`action_uri`、憑證欄位從 `text` 改成 `nvarchar(max)`，中文不會壞。

**修正（這次發現的）**
- 舊的排程寫入時沒有填帳號 ID（全部是空的 GUID）。
- **M1 的 CI 會失敗**：M1 之後程式預設是展示模式，`dotnet ef` 在設計時也跟著用 SQLite 建模型，所以「migration 跟模型一致」的檢查會失敗，產生的 migration 也會是 SQLite 的型別。
  - 加了 `DesignTimeDbContextFactory`，EF 工具一律用 SQL Server。
  - M1 沒有重跑這項檢查，所以當時沒發現。
- 註冊時可以順便綁官方帳號的那條路徑會跳過 LINE 驗證，已經拿掉。註冊只要基本資料，官方帳號登入後再新增。
- 圖文選單卡片點圖片會誤觸編輯，改成只從選單操作。

**調整**
- 左側選單分成「已完成」和「規劃中」：規劃中的項目照樣顯示（產品藍圖），標「規劃中」、滑過去看說明，但點不進去。原本那些「即將推出」的空頁面和路由都拿掉了。
- 建立圖文選單時順便排程的欄位拿掉了，排程統一走排程 API。
  - 原因：`RichMenuDTO` 會原封不動送給 LINE，裡面混了非 LINE 格式的排程欄位。
- migration：`M2_SchedulingAndAccountManagement`。

**已知限制**
- 排程背景服務假設單機執行：多台同時跑會重複處理。要水平擴充時，再換成有分散式鎖的排程器（Quartz 叢集模式或 Hangfire）。
- Data Protection 的金鑰本身在資料庫裡沒有再加密一層，啟動時會出現 `No XML encryptor configured` 警告。
  - 正式環境建議用 `ProtectKeysWithCertificate` 或 Azure Key Vault 保護金鑰。
  - 展示模式不需要。

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

### M1 時的已知問題（都在 M2 修正）

- 圖文選單的本地紀錄 ID（自己產生的 GUID）跟 LINE 的 ID（`richmenu-…`）對不上，刪除時本地紀錄刪不掉。
- 找不到圖文選單、沒有預設選單時丟 `InvalidOperationException` 或 LINE 的 404，前端會跳錯誤訊息。
- SQL Server 的 `action_data`、`action_uri` 欄位是 `text`（非 Unicode），存中文會壞。要改成 `nvarchar(max)`。
