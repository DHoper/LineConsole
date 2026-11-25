import { create } from 'zustand';
import { persist } from 'zustand/middleware';

import { useAuth } from '@/features/auth/stores/useAuth';

import { useAccounts } from '../api/accountsAPI';

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

/** 目前的官方帳號：選過的那個；沒選過、或已經被刪掉，就用第一個；還沒有任何帳號是 null */
export function useCurrentAccount() {
    const { isAuthenticated } = useAuth();
    const { data: accounts = [] } = useAccounts(isAuthenticated);
    const accountId = useCurrentAccountStore((s) => s.accountId);
    return accounts.find((a) => a.id === accountId) ?? accounts[0] ?? null;
}
