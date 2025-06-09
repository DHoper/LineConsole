import axios from "axios";

import { handleError } from "@/libs/utils/handleError";

/**
 * 全域 Axios 實例，用於統一 API 呼叫設定與攔截
 */
export const axiosClient = axios.create({
    baseURL: "/api", 
    timeout: 10000,
});

// 統一錯誤處理（所有非 2xx response 都會進來）
axiosClient.interceptors.response.use(
    (response) => response,
    (error) => {
        handleError(error);
        return Promise.reject(error);
    }
);
