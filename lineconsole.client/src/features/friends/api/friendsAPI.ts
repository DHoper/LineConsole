import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { ApiResponse } from '@/features/common/types/api';
import { axiosClient } from '@/libs/http/axiosClient';

export type FriendStatus = 'Following' | 'Blocked';

/** 好友（資料來自 LINE Webhook 的 follow／unfollow／message 事件） */
export interface Friend {
    id: string;
    lineUserId: string;
    displayName: string | null;
    pictureUrl: string | null;
    status: FriendStatus;
    followedAt: string;
    blockedAt: string | null;
    lastMessageAt: string | null;
}

export interface PagedResult<T> {
    items: T[];
    total: number;
    page: number;
    pageSize: number;
}

export interface FriendQuery {
    status?: 'following' | 'blocked';
    keyword?: string;
    page: number;
    pageSize: number;
}

/** 某一天（台北時間）的新增、封鎖，以及當天結束時的好友人數 */
export interface DailyFriendStat {
    date: string;
    follows: number;
    unfollows: number;
    following: number;
}

export interface Dashboard {
    following: number;
    blocked: number;
    followsToday: number;
    unfollowsToday: number;
    /** 近 30 天，舊的在前 */
    daily: DailyFriendStat[];
    richMenuCount: number;
    pendingSchedules: number;
    activeSchedules: number;
}

export type SimulateKind = 'follow' | 'unfollow' | 'message' | 'redelivery';

export interface SimulateResult {
    kind: SimulateKind;
    accepted: number;
    duplicates: number;
    webhookEventId: string;
    lineUserId: string | null;
}

export const friendsAPI = {
    list: async (accountId: string, q: FriendQuery): Promise<ApiResponse<PagedResult<Friend>>> =>
        (await axiosClient.get(`/accounts/${accountId}/friends`, { params: q })).data,
    dashboard: async (accountId: string): Promise<ApiResponse<Dashboard>> =>
        (await axiosClient.get(`/accounts/${accountId}/dashboard`)).data,
    simulate: async (accountId: string, kind: SimulateKind): Promise<ApiResponse<SimulateResult>> =>
        (await axiosClient.post(`/accounts/${accountId}/demo/webhook-events`, { kind })).data,
};

const keys = {
    friends: (accountId: string) => ['friends', accountId],
    dashboard: (accountId: string) => ['dashboard', accountId],
};

/** 好友名單：換頁、篩選時保留上一頁的資料，不閃空白 */
export const useFriends = (accountId: string, q: FriendQuery) =>
    useQuery({
        queryKey: [...keys.friends(accountId), q],
        queryFn: () => friendsAPI.list(accountId, q),
        select: (res) => res.data,
        enabled: !!accountId,
        placeholderData: keepPreviousData,
    });

export const useDashboard = (accountId: string) =>
    useQuery({
        queryKey: keys.dashboard(accountId),
        queryFn: () => friendsAPI.dashboard(accountId),
        select: (res) => res.data,
        enabled: !!accountId,
        placeholderData: keepPreviousData,
    });

/**
 * 展示模式：扮演 LINE 送一個 Webhook 事件。
 * 事件是背景服務非同步處理的，所以送出後隔一下再重抓儀表板和好友名單。
 */
export const useSimulateWebhook = (accountId: string) => {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (kind: SimulateKind) => friendsAPI.simulate(accountId, kind),
        onSuccess: () => {
            const refresh = () => {
                qc.invalidateQueries({ queryKey: keys.dashboard(accountId) });
                qc.invalidateQueries({ queryKey: keys.friends(accountId) });
            };
            setTimeout(refresh, 800);
            setTimeout(refresh, 2500);
        },
    });
};

/**
 * 送一個「簽章是亂填的」Webhook，證明後端會擋下來（401）。
 * 刻意不用 axiosClient：它收到 401 會當成登入過期把人登出。
 */
export async function sendForgedWebhook(accountId: string): Promise<{ status: number; code?: string; message?: string }> {
    const body = JSON.stringify({
        destination: 'Uforged',
        events: [{ type: 'follow', webhookEventId: 'FORGED' + Date.now(), timestamp: Date.now(), source: { type: 'user', userId: 'U' + '0'.repeat(32) } }],
    });
    const res = await fetch(`/api/webhooks/line/${accountId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Line-Signature': btoa('not-signed-by-line-not-signed-by') },
        body,
    });
    const json = (await res.json().catch(() => null)) as ApiResponse<unknown> | null;
    return { status: res.status, code: json?.error?.code, message: json?.error?.message };
}
