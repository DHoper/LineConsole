import { axiosClient } from "@/libs/http/axiosClient";
import { ApiResponse } from "@/types/api";
import { RegisterRequest, LoginRequest, UserProfileDTO } from "../types";

/** 提供註冊、登入與取得目前登入者資訊的 API 呼叫函式 */
export const authAPI = {
    /** 使用者註冊 */
    register: async (request: RegisterRequest): Promise<ApiResponse<string>> => {
        const res = await axiosClient.post("/api/auth/register", request);
        return res.data;
    },

    /** 使用者登入，回傳 JWT Token */
    login: async (request: LoginRequest): Promise<ApiResponse<string>> => {
        const res = await axiosClient.post("/api/auth/login", request);
        return res.data;
    },

    /** 取得目前登入使用者資訊（需含 JWT） */
    me: async (): Promise<ApiResponse<UserProfileDTO>> => {
        const res = await axiosClient.get("/api/auth/me");
        return res.data;
    },
};
