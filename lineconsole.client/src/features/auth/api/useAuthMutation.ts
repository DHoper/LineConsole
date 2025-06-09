import { useMutation } from "@tanstack/react-query";
import { ApiResponse } from "@/types/api";
import { RegisterRequest, LoginRequest } from "../types";
import { authAPI } from "./authAPI";

/** 註冊帳號的 Mutation（回傳使用者 ID） */
export const useRegister = () =>
    useMutation<ApiResponse<string>, unknown, RegisterRequest>({
        mutationFn: authAPI.register,
    });

/** 登入帳號的 Mutation（回傳 JWT Token） */
export const useLogin = () =>
    useMutation<ApiResponse<string>, unknown, LoginRequest>({
        mutationFn: authAPI.login,
    });
