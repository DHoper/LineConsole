import { useQuery } from "@tanstack/react-query";
import { ApiResponse } from "@/types/api";
import {
    RichMenuIdResult,
    RichMenuResult,
    RichMenuWithImageResult,
} from "../types";
import { richMenuAPI } from "./richMenuAPI";

/** 取得 RichMenu 列表（含圖片預覽） */
export const useRichMenuList = (lineOfficialAccountId: string, enabled = true) => {
    return useQuery<ApiResponse<RichMenuWithImageResult[]>>({
        queryKey: ["line-richmenus", lineOfficialAccountId],
        queryFn: () => richMenuAPI.listWithPreview(lineOfficialAccountId),
        enabled: !!lineOfficialAccountId && enabled,
    });
};

/** 取得指定 RichMenu 詳細資訊 */
export const useRichMenuById = (
    lineOfficialAccountId: string,
    richMenuId: string,
    enabled = true
) => {
    return useQuery<ApiResponse<RichMenuResult>>({
        queryKey: ["line-richmenu", lineOfficialAccountId, richMenuId],
        queryFn: () => richMenuAPI.getById(lineOfficialAccountId, richMenuId),
        enabled: !!lineOfficialAccountId && !!richMenuId && enabled,
    });
};

/** 取得目前預設的 RichMenu */
export const useDefaultRichMenu = (
    lineOfficialAccountId: string,
    enabled = true
) => {
    return useQuery<ApiResponse<RichMenuIdResult>>({
        queryKey: ["line-richmenu-default", lineOfficialAccountId],
        queryFn: () => richMenuAPI.getDefault(lineOfficialAccountId),
        enabled: !!lineOfficialAccountId && enabled,
    });
};
