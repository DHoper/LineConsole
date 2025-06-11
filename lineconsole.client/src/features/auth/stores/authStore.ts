import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

export interface LineAccount {
    id: string;
    channelId: string;
    channelName: string;
}

export interface AuthUser {
    userId: string;
    email: string;
    displayName?: string;
    role: string;
    lineAccounts: LineAccount[];
}

interface AuthState {
    token: string | null;
    user: AuthUser | null;
    expiresAt: number | null; // 後端回傳的 JWT 過期時間（Unix 秒數）
}

interface AuthActions {
    /**
     * 設定登入資訊，包含 token、user 與過期時間
     * @param token JWT 字串
     * @param user 使用者資料
     * @param expiresAt token 過期時間（Unix 秒）
     */
    login: (token: string, user: AuthUser, expiresAt: number) => void;

    /** 清除登入資訊 */
    logout: () => void;

    /** 判斷是否已過期 */
    isExpired: () => boolean;
}

type AuthStore = AuthState & AuthActions;

export const useAuthStore = create<AuthStore>()(
    persist<AuthStore>(
        (set, get) => ({
            token: null,
            user: null,
            expiresAt: null,

            login: (token: string, user: AuthUser, expiresAt: number) => {
                set({ token, user, expiresAt });
            },

            logout: () => {
                set({ token: null, user: null, expiresAt: null });
            },

            isExpired: () => {
                const expiresAt = get().expiresAt;
                return typeof expiresAt === 'number' && Date.now() / 1000 > expiresAt;
            },
        }),
        {
            name: 'auth-storage',
            storage: createJSONStorage(() => localStorage),
        }
    )
);
