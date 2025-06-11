/** 註冊請求資料 */
export interface RegisterInput {
  email: string;                          // 使用者登入 Email
  password: string;                       // 原始密碼
  displayName?: string;                   // 顯示名稱（可選）
  lineAccount?: LineOfficialAccountCreateModel; // 綁定的 LINE 官方帳號資訊（可選）
}

/** 登入請求資料 */
export interface LoginInput {
  email: string;      // 使用者登入 Email
  password: string;   // 原始密碼
}

/** LINE 官方帳號建立所需資料 */
export interface LineOfficialAccountCreateModel {
  channelName: string;             // LINE 官方帳號名稱
  channelId: string;               // LINE Channel ID
  channelSecret: string;           // Channel Secret
  channelAccessToken: string;      // Channel Access Token
}

/** 登入結果：包含 JWT Token 與使用者資訊 */
export interface LoginResult {
  token: string;        // JWT 權杖
  expiresAt: number;    // 過期時間
  user: UserLoginInfo;  // 登入使用者資料
}

/** 登入後取得的使用者資訊 */
export interface UserLoginInfo {
  userId: string;       // Identity 使用者 ID
  email: string;        // 使用者 Email
  displayName?: string; // 顯示名稱（可選）
  role: string;         // 使用者角色
  lineAccounts: {
    id: string;             // LINE 官方帳號 ID
    channelId: string;      // Channel ID
    channelName: string;    // Channel 名稱
  }[];
}
