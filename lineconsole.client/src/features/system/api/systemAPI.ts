import { useQuery } from '@tanstack/react-query';

import { ApiResponse } from '@/features/common/types/api';
import { axiosClient } from '@/libs/http/axiosClient';

export interface SystemInfo {
    mode: 'Demo' | 'Live';
    /** 展示模式才有：公開的展示帳號 */
    demoAccount: { email: string; password: string } | null;
}

export const systemAPI = {
    info: async (): Promise<ApiResponse<SystemInfo>> => {
        const res = await axiosClient.get('/system/info');
        return res.data;
    },
};

/** 系統資訊（執行模式）：整個程式執行期間不會變，只抓一次 */
export const useSystemInfo = () =>
    useQuery({
        queryKey: ['system-info'],
        queryFn: systemAPI.info,
        select: (res) => res.data,
        staleTime: Infinity,
    });
