import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ApiResponse } from "@/features/common/types/api";
import {
    RichMenuIdResult,
    CreateRichMenuInput,
} from "../types";
import { richMenuAPI } from "./richMenuAPI";

// 建立 RichMenu（包含圖片）
export const useCreateRichMenuWithImage = (lineOfficialAccountId: string) => {
    const queryClient = useQueryClient();

    return useMutation<
        ApiResponse<RichMenuIdResult>,
        unknown,
        CreateRichMenuInput
    >({
        mutationFn: (request) =>
            richMenuAPI.createWithImage(lineOfficialAccountId, request),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["line-richmenus", lineOfficialAccountId] });
        },
    });
};

// 刪除 Rich Menu
export const useDeleteRichMenu = (lineOfficialAccountId: string) => {
    const queryClient = useQueryClient();

    return useMutation<ApiResponse<null>, unknown, string>({
        mutationFn: (richMenuId) => richMenuAPI.delete(lineOfficialAccountId, richMenuId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["line-richmenus", lineOfficialAccountId] });
        },
    });
};

// 設定預設 Rich Menu
export const useSetDefaultRichMenu = (lineOfficialAccountId: string) => {
    const queryClient = useQueryClient();

    return useMutation<ApiResponse<null>, unknown, string>({
        mutationFn: (richMenuId) => richMenuAPI.setDefault(lineOfficialAccountId, richMenuId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["line-richmenu-default", lineOfficialAccountId] });
            queryClient.invalidateQueries({ queryKey: ["line-richmenus", lineOfficialAccountId] });
        },
    });
};

// 清除預設 Rich Menu
export const useClearDefaultRichMenu = (lineOfficialAccountId: string) => {
    const queryClient = useQueryClient();

    return useMutation<ApiResponse<null>>({
        mutationFn: () => richMenuAPI.clearDefault(lineOfficialAccountId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["line-richmenu-default", lineOfficialAccountId] });
            queryClient.invalidateQueries({ queryKey: ["line-richmenus", lineOfficialAccountId] });
        },
    });
};
