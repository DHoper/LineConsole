import { Bell, Check, ChevronDown, LogOut, User } from "lucide-react";
import { useNavigate } from "react-router-dom";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
    DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/features/auth/stores/useAuth";
import { cn } from "@/libs/ui/utils";

// 簡易取得使用者名稱首字母
const getInitial = (name?: string) => {
    if (!name) return "U";
    const char = name.trim().charAt(0);
    return /^[A-Za-z0-9一-龥]$/.test(char) ? char.toUpperCase() : "U";
};

export default function TheHeader() {
    const navigate = useNavigate();
    const { user, logout, isExpired } = useAuth();

    const isAuthenticated = !!user && !isExpired();
    const currentChannel = user?.lineAccounts?.[0];

    const handleLogout = () => {
        logout();
        navigate("/auth/login");
    };

    return (
        <header className="flex items-center justify-between h-16 px-6 bg-background border-b border-border">
            {/* ✅ 左側：官方帳號切換 */}
            <div className="flex items-center gap-3 min-w-0">
                {isAuthenticated && currentChannel ? (
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button
                                variant="ghost"
                                className="h-10 px-3 flex items-center gap-2 rounded-md transition-colors hover:bg-accent"
                            >
                                <div className="w-2.5 h-2.5 rounded-full bg-green-500" />
                                <span
                                    className="truncate max-w-[10rem] text-sm font-medium"
                                    title={currentChannel.channelName}
                                >
                                    {currentChannel.channelName}
                                </span>
                                <ChevronDown className="w-4 h-4 opacity-70" />
                            </Button>
                        </DropdownMenuTrigger>

                        {/* ✅ 關鍵：寬度對齊觸發器 */}
                        <DropdownMenuContent
                            align="start"
                            sideOffset={4}
                            className="min-w-[var(--radix-dropdown-menu-trigger-width)] w-auto"
                        >
                            {user.lineAccounts.map((account) => {
                                const isActive = account.id === currentChannel.id;
                                return (
                                    <DropdownMenuItem
                                        key={account.id}
                                        onClick={() => console.log("切換帳號", account.channelName)}
                                        className={cn(
                                            "flex items-center justify-between px-3 py-2 rounded-md cursor-pointer",
                                            isActive && "bg-muted"
                                        )}
                                    >
                                        <span className="truncate">{account.channelName}</span>
                                        {isActive && <Check className="w-4 h-4 text-primary" />}
                                    </DropdownMenuItem>
                                );
                            })}
                        </DropdownMenuContent>
                    </DropdownMenu>
                ) : (
                    <span className="text-muted-foreground text-sm">尚未登入</span>
                )}
            </div>

            {/* ✅ 右側：通知 + 使用者下拉 */}
            <div className="flex items-center gap-3">
                <Button variant="ghost" size="icon" className="rounded-full hover:bg-accent h-10 w-10">
                    <Bell className="w-5 h-5" />
                </Button>

                {isAuthenticated ? (
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button
                                variant="ghost"
                                size="icon"
                                className="rounded-full h-10 w-10 transition-colors hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
                            >
                                <div className="w-8 h-8 rounded-full bg-muted flex items-center justify-center text-sm font-semibold">
                                    {getInitial(user.displayName ?? user.email)}
                                </div>
                            </Button>
                        </DropdownMenuTrigger>

                        <DropdownMenuContent align="end" className="w-56">
                            {/* 顯示用戶資訊區 */}
                            <div className="px-3 py-2">
                                <p className="text-sm font-medium leading-none truncate">
                                    {user.displayName ?? "未命名使用者"}
                                </p>
                                <p className="text-xs text-muted-foreground truncate">{user.email}</p>
                            </div>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem onClick={() => navigate("/profile")} className="px-3 py-2">
                                <User className="w-4 h-4 mr-2" />
                                個人資料
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={handleLogout} className="px-3 py-2">
                                <LogOut className="w-4 h-4 mr-2" />
                                登出
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                ) : (
                    <Button size="sm" onClick={() => navigate("/auth/login")}>
                        登入
                    </Button>
                )}
            </div>
        </header>
    );
}
