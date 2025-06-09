/** 註冊請求格式 */
export interface RegisterRequest {
  email: string;        // 登入用 Email
  password: string;     // 原始密碼
}

/** 登入請求格式 */
export interface LoginRequest {
  email: string;        // 登入用 Email
  password: string;     // 原始密碼
}

/** 回傳的使用者資料格式 */
export interface UserProfileDTO {
  id: string;           // 使用者主鍵 ID
  name?: string;        // 使用者暱稱（可選）
  createdAt: string;    // 建立時間（ISO 格式）
  updatedAt: string;    // 更新時間（ISO 格式）
}
