import {
    Check,
    Clock,
    Edit3,
    MoreVertical,
    Trash2,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Empty } from "@/components/ui/empty";

import { type RichMenuWithImageResult } from "../types";

type Props = {
  menu: RichMenuWithImageResult;
  isDefault: boolean;
  hasImageError: boolean;
  onEdit: () => void;
  onDelete: () => void;
  onToggleDefault: () => void;
  onImageError: () => void;

  /** 🟡 父層計算好的動作摘要（例如："2×postback、1×message"） */
  actionTypesSummary?: string;
};

export default function RichMenuCard({
    menu,
    isDefault,
    hasImageError,
    onEdit,
    onDelete,
    onToggleDefault,
    onImageError,
    actionTypesSummary,
}: Props) {
    const aspectRatio = menu.width / menu.height;

    // 🗓️ 假資料，未來改為傳入排程
    const fakeScheduleTime = "2025-06-01T14:30:00Z";
    const formattedSchedule = new Intl.DateTimeFormat("zh-TW", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
    }).format(new Date(fakeScheduleTime));

    return (
        <div className="group border rounded-xl bg-white dark:bg-card shadow-sm hover:shadow-md transition overflow-hidden">
            <div
                className="relative bg-muted cursor-pointer"
                style={{ aspectRatio }}
                onClick={onEdit}
            >
                {!hasImageError ? (
                    <img
                        src={`data:image/jpeg;base64,${menu.imageBase64}`}
                        alt={menu.name}
                        className="absolute inset-0 w-full h-full object-cover"
                        onError={onImageError}
                    />
                ) : (
                    <div className="absolute inset-0 flex items-center justify-center">
                        <Empty title="尚無選單圖片" description="" icon={null} />
                    </div>
                )}

                {isDefault && (
                    <Badge
                        variant="default"
                        className="absolute top-2 left-2 text-xs font-medium px-2 py-0.5 shadow"
                    >
            ✅ 目前預設
                    </Badge>
                )}

                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button
                            size="icon"
                            variant="ghost"
                            className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition"
                            aria-label="更多設定"
                        >
                            <MoreVertical className="w-4 h-4" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" sideOffset={4}>
                        <DropdownMenuItem onClick={onEdit}>
                            <Edit3 className="w-4 h-4 mr-2" />
              編輯
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={onToggleDefault}>
                            {isDefault ? (
                                <>
                                    <Check className="w-4 h-4 mr-2" />
                  目前預設
                                </>
                            ) : (
                                <>設為預設</>
                            )}
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={onDelete} className="text-destructive">
                            <Trash2 className="w-4 h-4 mr-2" />
              刪除
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>

            <div className="p-4 space-y-2">
                <h2 className="text-base font-semibold text-foreground">{menu.name}</h2>
                <p className="text-sm text-muted-foreground">
          類型：{actionTypesSummary || "未提供"}
                </p>
                <div className="text-sm text-muted-foreground flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    <span className="text-xs">{formattedSchedule}</span>
                </div>
            </div>
        </div>
    );
}
