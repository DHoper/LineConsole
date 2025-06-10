import { Bell, Search, LogOut, User } from "lucide-react";
import { useNavigate } from "react-router-dom";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

export default function TheHeader() {
    const navigate = useNavigate();

    // 假資料，可換成全域狀態（Zustand 等）
    const isAuthenticated = true;
    const user = {
        name: "小王",
        avatar: "https://i.pravatar.cc/32",
    };

    const handleLogout = () => {
        // 清除登入狀態
        console.log("🚪 登出");
        navigate("/auth/login");
    };

    const handleLogin = () => {
        navigate("/auth/login");
    };

    return (
        <header className="flex items-center justify-between h-16 px-6 bg-white dark:bg-background border-b border-border shadow-sm">
            {/* 左側：帳號名稱 */}
            <div className="text-lg font-semibold text-primary truncate max-w-[50%]">
                {isAuthenticated ? "官方帳號：小王的 LINE 帳號" : "尚未登入"}
            </div>

            {/* 右側：功能區塊 */}
            <div className="flex items-center gap-4">
                <button className="p-2 rounded-md hover:bg-gray-100 dark:hover:bg-muted transition">
                    <Search className="w-5 h-5 text-gray-600 dark:text-gray-300" />
                </button>
                <button className="p-2 rounded-md hover:bg-gray-100 dark:hover:bg-muted transition">
                    <Bell className="w-5 h-5 text-gray-600 dark:text-gray-300" />
                </button>

                {isAuthenticated ? (
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Avatar className="cursor-pointer">
                                <AvatarImage src={user.avatar} alt={user.name} />
                                <AvatarFallback>{user.name[0]}</AvatarFallback>
                            </Avatar>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => navigate("/profile")}>
                                <User className="w-4 h-4 mr-2" />
                                個人資料
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={handleLogout}>
                                <LogOut className="w-4 h-4 mr-2" />
                                登出
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                ) : (
                    <Button size="sm" onClick={handleLogin}>
                        登入
                    </Button>
                )}
            </div>
        </header>
    );
}
