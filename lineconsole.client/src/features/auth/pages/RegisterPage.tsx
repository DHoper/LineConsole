// src/features/auth/pages/RegisterPage.tsx

import { useState } from "react";
import { useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import * as yup from "yup";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { authAPI } from "@/features/auth/api/authAPI";
import { RegisterInput } from "@/features/auth/types";

import {
    Card,
    CardHeader,
    CardTitle,
    CardContent,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";

// 表單欄位（UI 專用）
interface RegisterFormData {
    displayName: string;
    email: string;
    password: string;
    confirmPassword: string;
    channelName: string;
    channelId: string;
    channelSecret: string;
    channelAccessToken: string;
}

// 驗證規則
const schema: yup.ObjectSchema<RegisterFormData> = yup.object({
    displayName: yup.string().required("姓名為必填"),
    email: yup.string().email("Email 格式錯誤").required("Email 為必填"),
    password: yup.string().min(6, "密碼至少 6 位").required("密碼為必填"),
    confirmPassword: yup
        .string()
        .oneOf([yup.ref("password")], "密碼不一致")
        .required("請再次輸入密碼"),
    channelName: yup.string().required("LINE 官方帳號名稱為必填"),
    channelId: yup.string().required("Channel ID 為必填"),
    channelSecret: yup.string().required("Channel Secret 為必填"),
    channelAccessToken: yup.string().required("Access Token 為必填"),
});

export default function RegisterPage() {
    const navigate = useNavigate();
    const [step, setStep] = useState(1);

    const {
        register,
        handleSubmit,
        formState: { errors },
        trigger,
    } = useForm<RegisterFormData>({
        resolver: yupResolver(schema),
        mode: "onTouched",
    });

    const transformToRegisterInput = (form: RegisterFormData): RegisterInput => ({
        email: form.email,
        password: form.password,
        displayName: form.displayName,
        lineAccount: {
            channelName: form.channelName,
            channelId: form.channelId,
            channelSecret: form.channelSecret,
            channelAccessToken: form.channelAccessToken,
        },
    });

    const onSubmit = async (form: RegisterFormData) => {
        const input = transformToRegisterInput(form);
        try {
            const res = await authAPI.register(input);
            if (res.success) {
                toast.success("註冊成功，請登入");
                navigate("/auth/login");
            } else {
                toast.error(res.error?.message || "註冊失敗");
                console.warn("Register failed:", res.error);
            }
        } catch (err) {
            console.error("註冊錯誤：", err);
            toast.error("系統錯誤，請稍後再試");
        }
    };

    const goToStep2 = async () => {
        const valid = await trigger(["displayName", "email", "password", "confirmPassword"]);
        if (valid) setStep(2);
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-muted/40 px-4">
            <Card className="w-full max-w-xl">
                <CardHeader className="space-y-1 pb-0">
                    <CardTitle className="text-2xl text-center">
                        註冊帳號（{step === 1 ? "基本資訊" : "LINE 資訊"}）
                    </CardTitle>
                    <p className="text-sm text-muted-foreground text-center">
                        {step === 1 ? "請填寫您的基本資訊" : "請填寫 LINE 串接資訊"}
                    </p>
                </CardHeader>

                <CardContent className="pt-4 space-y-6">
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                        {step === 1 ? (
                            <>
                                <div className="space-y-2">
                                    <Label htmlFor="displayName">姓名</Label>
                                    <Input id="displayName" {...register("displayName")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.displayName?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="email">Email</Label>
                                    <Input id="email" type="email" {...register("email")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.email?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="password">密碼</Label>
                                    <Input id="password" type="password" {...register("password")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.password?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="confirmPassword">確認密碼</Label>
                                    <Input id="confirmPassword" type="password" {...register("confirmPassword")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.confirmPassword?.message ?? "\u00A0"}</p>
                                </div>

                                <Button type="button" className="w-full" onClick={goToStep2}>
                                    下一步：填寫 LINE 設定
                                </Button>
                            </>
                        ) : (
                            <>
                                <div className="space-y-2">
                                    <Label htmlFor="channelName">LINE 官方帳號名稱</Label>
                                    <Input id="channelName" {...register("channelName")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.channelName?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="channelId">Channel ID</Label>
                                    <Input id="channelId" {...register("channelId")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.channelId?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="channelSecret">Channel Secret</Label>
                                    <Input id="channelSecret" {...register("channelSecret")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.channelSecret?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="channelAccessToken">Access Token</Label>
                                    <Textarea id="channelAccessToken" {...register("channelAccessToken")} />
                                    <p className="text-sm text-destructive min-h-[20px]">{errors.channelAccessToken?.message ?? "\u00A0"}</p>
                                </div>

                                <div className="flex justify-between gap-2 pt-2">
                                    <Button type="button" variant="secondary" onClick={() => setStep(1)}>
                                        上一步
                                    </Button>
                                    <Button type="submit" className="flex-1">
                                        建立帳號
                                    </Button>
                                </div>
                            </>
                        )}

                        <p className="text-sm text-center text-muted-foreground pt-2">
                            已有帳號？{" "}
                            <a href="/auth/login" className="text-primary underline">
                                前往登入
                            </a>
                        </p>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}
