import axios, {
    AxiosError,
    AxiosHeaders,
    AxiosInstance,
    InternalAxiosRequestConfig
} from 'axios';
import { useAuthStore } from '@/features/auth/stores/authStore';
import { handleError } from '@/libs/utils/handleError';

const axiosClient: AxiosInstance = axios.create({
    baseURL: "/api", 
    timeout: 10000,
});

axiosClient.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
        const token = useAuthStore.getState().token;

        if (token) {
            if (config.headers instanceof AxiosHeaders) {
                config.headers.set('Authorization', `Bearer ${token}`);
            } else {
                config.headers = new AxiosHeaders();
                config.headers.set('Authorization', `Bearer ${token}`);
            }
        }

        return config;
    },
    async (error: AxiosError) => {
        return await Promise.reject(error);
    }
);

// 統一處理 401/403 或其他錯誤
axiosClient.interceptors.response.use(
    async (response) => response,
    async (error: AxiosError) => {
        const status = error.response?.status;

        if (status === 401 || status === 403) {
            useAuthStore.getState().logout();
            window.location.href = '/auth/login';
        }

        handleError(error);
        return await Promise.reject(error);
    }
);

export { axiosClient };
