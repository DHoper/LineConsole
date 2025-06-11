import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { authAPI } from "@/features/auth/api/authAPI";
import { useAuth } from "@/features/auth/stores/useAuth";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";

export default function LoginPage() {
    const navigate = useNavigate();
    const { login } = useAuth(); 

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    const handleLogin = async () => {
        if (!email || !password) {
            toast.warning("請填寫帳號與密碼");
            return;
        }

        setLoading(true);
        try {
            const res = await authAPI.login({ email, password });
            if (res.success && res.data) {
                const { token, user, expiresAt } = res.data;
                login(token, user, expiresAt);

                toast.success("登入成功");
                
                navigate("/dashboard");
            } else {
                toast.error(res.error?.message ?? "登入失敗");
            }

        } catch (err) {
            console.error("登入錯誤：", err);
            toast.error("系統錯誤，請稍後再試");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-muted/40 px-4">
            <Card className="w-full max-w-md shadow-md border border-border">
                <CardHeader className="space-y-1 pb-0">
                    <CardTitle className="text-2xl text-center">登入帳號</CardTitle>
                    <p className="text-sm text-muted-foreground text-center">
                        請輸入帳號與密碼以繼續使用
                    </p>
                </CardHeader>

                <CardContent className="pt-4 space-y-6">
                    <div className="space-y-2">
                        <Label htmlFor="email">電子郵件</Label>
                        <Input
                            id="email"
                            type="email"
                            placeholder="you@example.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            disabled={loading}
                        />
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="password">密碼</Label>
                        <Input
                            id="password"
                            type="password"
                            placeholder="請輸入密碼"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            disabled={loading}
                        />
                    </div>

                    <Button className="w-full mt-2" onClick={handleLogin} disabled={loading}>
                        {loading ? "登入中..." : "登入"}
                    </Button>

                    <p className="text-sm text-center text-muted-foreground">
                        還沒有帳號嗎？{" "}
                        <a href="/auth/register" className="text-primary underline">
                            前往註冊
                        </a>
                    </p>
                </CardContent>
            </Card>
        </div>
    );
}
