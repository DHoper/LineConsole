import type { AxiosError } from "axios";
import { toast } from "sonner";

/** 後端 ApiResponse 失敗時的格式：{ success: false, error: { code, message } } */
interface ErrorResponse {
    error?: { code?: string; message?: string }
    message?: string
}

/**
 * 統一處理 API 錯誤訊息（支援 AxiosError 與一般錯誤）
 */
export function handleError(error: unknown) {
    let message = "發生未知錯誤，請稍後再試。";

    if (typeof error === "string") {
        message = error;
    } else if (
        typeof error === "object" &&
        error !== null &&
        "message" in error &&
        typeof (error as { message?: string }).message === "string"
    ) {
        message = (error as { message: string }).message;
    }

    if ((error as AxiosError).isAxiosError) {
        const axiosErr = error as AxiosError<ErrorResponse>;
        const data = axiosErr.response?.data;
        const serverMessage = data?.error?.message ?? data?.message;
        if (serverMessage) {
            message = serverMessage;
        }
    }

    toast.error(message);
}
