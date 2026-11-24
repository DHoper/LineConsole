import { create } from 'zustand';
import { persist } from 'zustand/middleware';

import { useAuth } from '@/features/auth/stores/useAuth';

interface CurrentAccountState {
    accountId: string | null;
    setAccountId: (id: string) => void;
}

/** 目前操作的 LINE 官方帳號（記在瀏覽器，重新整理後還在） */
export const useCurrentAccountStore = create<CurrentAccountState>()(
    persist(
        (set) => ({
            accountId: null,
            setAccountId: (id) => set({ accountId: id }),
        }),
        { name: 'lineconsole-current-account' },
    ),
);

/** 目前的官方帳號：選過的那個；沒選過、或已經不是自己的帳號，就用第一個 */
export function useCurrentAccount() {
    const { user } = useAuth();
    const accountId = useCurrentAccountStore((s) => s.accountId);
    const accounts = user?.lineAccounts ?? [];
    return accounts.find((a) => a.id === accountId) ?? accounts[0] ?? null;
}
