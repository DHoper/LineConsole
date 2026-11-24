import { registerAuthHooks } from '@/libs/http/axiosClient';

import { useAuthStore } from './stores/authStore';

/** 把登入狀態接到共用的 HTTP 客戶端（main.tsx 啟動時呼叫一次） */
export function setupAuth() {
    registerAuthHooks({
        getToken: () => useAuthStore.getState().token,
        onUnauthorized: () => {
            useAuthStore.getState().logout();
            window.location.href = '/auth/login';
        },
    });
}
