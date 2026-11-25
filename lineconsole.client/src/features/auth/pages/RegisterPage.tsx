import { yupResolver } from "@hookform/resolvers/yup";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import * as yup from "yup";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { authAPI } from "@/features/auth/api/authAPI";

interface RegisterFormData {
    displayName: string;
    email: string;
    password: string;
    confirmPassword: string;
}

const schema: yup.ObjectSchema<RegisterFormData> = yup.object({
    displayName: yup.string().required("姓名為必填"),
    email: yup.string().email("Email 格式錯誤").required("Email 為必填"),
    password: yup
        .string()
        .min(6, "密碼至少 6 位")
        .matches(/\d/, "密碼至少要有一個數字")
        .required("密碼為必填"),
    confirmPassword: yup
        .string()
        .oneOf([yup.ref("password")], "密碼不一致")
        .required("請再次輸入密碼"),
});

/** 註冊：只要基本資料。LINE 官方帳號登入後再從「官方帳號」新增（會先向 LINE 驗證 token） */
export default function RegisterPage() {
    const navigate = useNavigate();
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<RegisterFormData>({ resolver: yupResolver(schema), mode: "onTouched" });

    const onSubmit = async (form: RegisterFormData) => {
        try {
            const res = await authAPI.register({
                email: form.email,
                password: form.password,
                displayName: form.displayName,
            });
            if (res.success) {
                toast.success("註冊成功，請登入後新增 LINE 官方帳號");
                navigate("/auth/login");
            } else {
                toast.error(res.error?.message ?? "註冊失敗");
            }
        } catch {
            // 錯誤訊息由 HTTP 客戶端統一顯示
        }
    };

    const field = (name: keyof RegisterFormData, label: string, type = "text") => (
        <div className="space-y-2">
            <Label htmlFor={name}>{label}</Label>
            <Input id={name} type={type} {...register(name)} disabled={isSubmitting} />
            <p className="text-sm text-destructive min-h-[20px]">{errors[name]?.message ?? " "}</p>
        </div>
    );

    return (
        <div className="min-h-screen flex items-center justify-center bg-muted/40 px-4">
            <Card className="w-full max-w-md shadow-md border border-border">
                <CardHeader className="space-y-1 pb-0">
                    <CardTitle className="text-2xl text-center">註冊帳號</CardTitle>
                    <p className="text-sm text-muted-foreground text-center">
                        LINE 官方帳號可以在登入後再綁定
                    </p>
                </CardHeader>

                <CardContent className="pt-4">
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-2">
                        {field("displayName", "姓名")}
                        {field("email", "Email", "email")}
                        {field("password", "密碼", "password")}
                        {field("confirmPassword", "確認密碼", "password")}

                        <Button type="submit" className="w-full" disabled={isSubmitting}>
                            {isSubmitting ? "建立中..." : "建立帳號"}
                        </Button>

                        <p className="text-sm text-center text-muted-foreground pt-4">
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
