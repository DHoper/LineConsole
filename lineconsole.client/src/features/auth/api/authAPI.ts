import { axiosClient } from "@/libs/http/axiosClient";
import { ApiResponse } from "@/features/common/types/api";
import { RegisterInput, LoginInput, LoginResult } from "../types";

/** 提供註冊與登入的 API 呼叫函式 */
export const authAPI = {
    /** 使用者註冊 */
    register: async (request: RegisterInput): Promise<ApiResponse<string>> => {
        const res = await axiosClient.post("/auth/register", request);
        return res.data;
    },

    /** 使用者登入，回傳 JWT Token 與使用者資訊 */
    login: async (request: LoginInput): Promise<ApiResponse<LoginResult>> => {
        const res = await axiosClient.post("/auth/login", request);
        return res.data;
    }
};
