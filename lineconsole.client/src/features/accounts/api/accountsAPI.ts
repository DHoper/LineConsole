import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { ApiResponse } from '@/features/common/types/api';
import { axiosClient } from '@/libs/http/axiosClient';

/** 官方帳號（不含 Secret、Access Token：這兩個只進不出） */
export interface OfficialAccount {
    id: string;
    channelName: string;
    channelId: string;
    basicId: string | null;
    createdAt: string;
}

export interface CreateOfficialAccountInput {
    channelName?: string;
    channelId: string;
    channelSecret: string;
    channelAccessToken: string;
}

export const accountsAPI = {
    list: async (): Promise<ApiResponse<OfficialAccount[]>> => (await axiosClient.get('/accounts')).data,
    create: async (input: CreateOfficialAccountInput): Promise<ApiResponse<OfficialAccount>> =>
        (await axiosClient.post('/accounts', input)).data,
    remove: async (id: string): Promise<ApiResponse<null>> => (await axiosClient.delete(`/accounts/${id}`)).data,
};

const KEY = ['accounts'];

/** 目前登入者的官方帳號（新增、刪除後會自動重抓） */
export const useAccounts = (enabled = true) =>
    useQuery({ queryKey: KEY, queryFn: accountsAPI.list, select: (res) => res.data ?? [], enabled });

export const useCreateAccount = () => {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: accountsAPI.create,
        onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
    });
};

export const useDeleteAccount = () => {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: accountsAPI.remove,
        onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
    });
};
