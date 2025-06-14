# LineConsole

LineConsole 是一個提供用戶串接並管理自己 LINE 官方帳號的後台服務平台。使用者可透過本系統進行行銷推播、自動化操作、圖文選單管理等多項功能，協助品牌有效經營 LINE 生態。

> 🚧 **本專案仍在開發中，尚無法部署運行。**

---

## 📦 專案簡介

- 支援用戶綁定與管理多個 LINE 官方帳號
- 提供自動化推播、圖文選單、行銷活動等模組
- 使用者友善的 Web UI 介面設計

---

## 🛠️ 技術架構

| 層級 | 技術 | 說明 |
|------|------|------|
| 後端 | ASP.NET Core (.NET 8, Clean Architecture 分層設計) |
| 前端 | Vue 3 + TypeScript |
| 資料庫 | SQL Server + EF Core |
| 認證 | ASP.NET Identity + JWT |
| 外部整合 | LINE Messaging API / LIFF | 串接 LINE 官方帳號服務功能 |

---

## 🚀 功能規劃（開發中）

- [x] 使用者註冊／登入
- [x] 綁定 LINE 官方帳號
- [x] Rich Menu 圖文選單 CRUD
- [x] 多帳號支援與切換
- [ ] 服務預約功能
- [ ] 推播訊息排程與發送
- [ ] 集點卡、抽獎等活動模組
- [ ] 後台管理員角色與權限控管

---
