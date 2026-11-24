import axios, {
    AxiosError,
    AxiosHeaders,
    AxiosInstance,
    InternalAxiosRequestConfig
} from 'axios';

import { handleError } from '@/libs/utils/handleError';

/**
 * 登入狀態的掛勾，由 auth 功能在啟動時註冊（見 features/auth/setupAuth.ts）。
 * 共用層（libs）不 import features：依賴方向是 features → libs，不是反過來。
 */
export interface AuthHooks {
    getToken: () => string | null;
    onUnauthorized: () => void;
}

let authHooks: AuthHooks = {
    getToken: () => null,
    onUnauthorized: () => {},
};

export function registerAuthHooks(hooks: AuthHooks) {
    authHooks = hooks;
}

const axiosClient: AxiosInstance = axios.create({
    baseURL: '/api',
    timeout: 10000,
});

axiosClient.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
        const token = authHooks.getToken();

        if (token) {
            if (!(config.headers instanceof AxiosHeaders)) {
                config.headers = new AxiosHeaders();
            }
            config.headers.set('Authorization', `Bearer ${token}`);
        }

        return config;
    },
    async (error: AxiosError) => {
        return await Promise.reject(error);
    }
);

// 401＝沒登入或 token 過期：登出回登入頁。403（權限不足）不登出，只顯示錯誤。
axiosClient.interceptors.response.use(
    async (response) => response,
    async (error: AxiosError) => {
        if (error.response?.status === 401) {
            authHooks.onUnauthorized();
        }

        handleError(error);
        return await Promise.reject(error);
    }
);

export { axiosClient };
