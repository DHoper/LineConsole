import { Plus } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useCurrentAccount } from "@/features/accounts/stores/currentAccount";

import { useClearDefaultRichMenu, useDeleteRichMenu, useSetDefaultRichMenu } from "../api/useRichMenuMutation";
import { useDefaultRichMenu, useRichMenuList, useRichMenuSchedules } from "../api/useRichMenuQuery";
import RichMenuCard from "../components/RichMenuCard";
import RichMenuEditor from "../components/RichMenuEditor";
import ScheduleDialog from "../components/ScheduleDialog";
import SchedulesPanel from "../components/SchedulesPanel";
import type { RichMenuSchedule } from "../types";
import { formatTime } from "../utils/schedule";

/** 卡片上的排程提示：進行中的優先，其次是最近一個待執行的 */
const scheduleLabelOf = (schedules: RichMenuSchedule[], richMenuId: string) => {
    const mine = schedules.filter((s) => s.richMenuId === richMenuId);
    const active = mine.find((s) => s.status === "Active");
    if (active) return `排程進行中，${formatTime(active.endTime)} 結束後還原`;
    const next = mine.filter((s) => s.status === "Pending").sort((a, b) => a.startTime.localeCompare(b.startTime))[0];
    return next ? `排程：${formatTime(next.startTime)} ～ ${formatTime(next.endTime)}` : undefined;
};

export default function RichMenuListPage() {
    const account = useCurrentAccount();
    const accountId = account?.id ?? "";
    const [editor, setEditor] = useState<{ open: boolean; templateId?: string }>({ open: false });
    const [scheduling, setScheduling] = useState<{ richMenuId: string; name: string } | null>(null);
    const [imageErrors, setImageErrors] = useState<Record<string, boolean>>({});

    const { data, isLoading, isError } = useRichMenuList(accountId);
    const { data: defaultMenu } = useDefaultRichMenu(accountId, !!accountId);
    const { data: schedules = [] } = useRichMenuSchedules(accountId);

    const deleteMutation = useDeleteRichMenu(accountId);
    const setDefaultMutation = useSetDefaultRichMenu(accountId);
    const clearDefaultMutation = useClearDefaultRichMenu(accountId);

    const handleToggleDefault = (id: string) => {
        if (id === defaultMenu?.data?.richMenuId) {
            clearDefaultMutation.mutate(undefined, { onSuccess: () => toast.success("預設選單已清除") });
        } else {
            setDefaultMutation.mutate(id, { onSuccess: () => toast.success("預設選單已更新") });
        }
    };

    if (!account) {
        return (
            <div className="p-6 max-w-6xl mx-auto">
                <h1 className="text-2xl font-bold mb-6">圖文選單管理</h1>
                <div className="text-center py-16 border rounded-lg bg-white space-y-4">
                    <p className="text-muted-foreground">還沒有綁定 LINE 官方帳號</p>
                    <Button asChild>
                        <Link to="/accounts">去綁定官方帳號</Link>
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="p-6 space-y-8 max-w-6xl mx-auto">
            <div className="flex items-start justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold">圖文選單管理</h1>
                    <p className="text-sm text-muted-foreground mt-1">官方帳號：{account.channelName}</p>
                </div>
                <Button onClick={() => setEditor({ open: true })}>
                    <Plus className="w-4 h-4 mr-2" />
                    新增選單
                </Button>
            </div>

            {isLoading ? (
                <p>載入中...</p>
            ) : isError ? (
                <p className="text-red-500">載入失敗，請稍後再試</p>
            ) : data?.data?.length ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {data.data.map((menu) => (
                        <RichMenuCard
                            key={menu.richMenuId}
                            menu={menu}
                            isDefault={defaultMenu?.data?.richMenuId === menu.richMenuId}
                            hasImageError={imageErrors[menu.richMenuId] || !menu.imageBase64}
                            scheduleLabel={scheduleLabelOf(schedules, menu.richMenuId)}
                            onEdit={() => setEditor({ open: true, templateId: menu.richMenuId })}
                            onSchedule={() => setScheduling({ richMenuId: menu.richMenuId, name: menu.name })}
                            onDelete={() => deleteMutation.mutate(menu.richMenuId, { onSuccess: () => toast.success("已刪除選單") })}
                            onToggleDefault={() => handleToggleDefault(menu.richMenuId)}
                            onImageError={() => setImageErrors((prev) => ({ ...prev, [menu.richMenuId]: true }))}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-20 text-gray-500 text-sm border rounded bg-white shadow">
                    尚未建立任何圖文選單
                </div>
            )}

            <SchedulesPanel accountId={accountId} schedules={schedules} />

            <Dialog open={editor.open} onOpenChange={(open) => setEditor(open ? editor : { open: false })}>
                <DialogContent className="max-w-5xl">
                    <DialogHeader>
                        <DialogTitle>{editor.templateId ? "以此為範本建立新選單" : "新增圖文選單"}</DialogTitle>
                    </DialogHeader>
                    {editor.open && (
                        <RichMenuEditor
                            key={editor.templateId ?? "new"}
                            accountId={accountId}
                            templateId={editor.templateId}
                            onSuccess={() => setEditor({ open: false })}
                        />
                    )}
                </DialogContent>
            </Dialog>

            <ScheduleDialog accountId={accountId} menu={scheduling} onClose={() => setScheduling(null)} />
        </div>
    );
}
