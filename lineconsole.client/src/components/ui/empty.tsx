import { Image as ImageIcon } from "lucide-react";

import { cn } from "@/libs/ui/utils";

type EmptyProps = {
    title?: string;
    description?: string;
    icon?: React.ReactNode | null; // 可傳入自訂 icon 或設為 null 隱藏
    className?: string;
};

export const Empty = ({
    title = "沒有圖片",
    description = "目前沒有可顯示的圖片內容",
    icon = <ImageIcon className="w-12 h-12 text-muted-foreground" />,
    className = "",
}: EmptyProps) => {
    return (
        <div className={cn("flex flex-col items-center justify-center py-12 text-center", className)}>
            {icon}
            <h3 className="mt-4 text-lg font-semibold">{title}</h3>
            <p className="mt-2 text-sm text-muted-foreground">{description}</p>
        </div>
    );
};
