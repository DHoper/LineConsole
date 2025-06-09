import { useQuery } from "@tanstack/react-query";
import { ApiResponse } from "@/types/api";
import { UserProfileDTO } from "../types";
import { authAPI } from "./authAPI";

/** 查詢目前登入使用者資訊（需攜帶 JWT） */
export const useUserProfile = () =>
    useQuery<ApiResponse<UserProfileDTO>>({
        queryKey: ["auth", "me"],
        queryFn: authAPI.me,
    });
