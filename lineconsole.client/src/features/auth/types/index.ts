/** 註冊請求格式 - 對應後端 RegisterInput */
export interface RegisterInput {
  email: string;                         // 登入用 Email
  password: string;                      // 原始密碼
  displayName?: string;                  // 顯示名稱（可選）
  avatarUrl?: string;                    // 頭像 URL（可選）
  organizationCode?: string;             // 所屬組織代碼（可選）
  lineAccount: LineOfficialAccountCreateModel; // 綁定的 LINE 官方帳號資訊
}

/** 登入請求格式 - 對應後端 LoginInput */
export interface LoginInput {
  email: string;        // 登入用 Email
  password: string;     // 原始密碼
}

/** LINE 官方帳號建立所需資料 - 對應後端 LineOfficialAccountCreateModel */
export interface LineOfficialAccountCreateModel {
  channelName: string;             // LINE 官方帳號名稱
  channelId: string;        // LINE Channel ID
  channelSecret: string;    // LINE Channel Secret
  channelAccessToken: string; // LINE Channel Access Token
}

/** 回傳的使用者資料格式（若未來登入後有用到） */
export interface UserProfileDTO {
  id: string;
  name?: string;
  createdAt: string;
  updatedAt: string;
}

/** 前端註冊表單處理用資料（UI 專用，與 API 無關） */
export interface RegisterFormData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  avatarUrl?: string;
  organizationCode?: string;
  channelName: string;
  channelId: string;
  channelSecret: string;
  channelToken: string;
}
