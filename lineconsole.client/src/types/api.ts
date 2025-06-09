// types/api.ts

export interface ApiResponse<T> {
    code: string; // 例如 "SUCCESS", "LINE_API_FAIL"
    message: string; // 顯示給使用者的訊息
    data: T | null;
}
