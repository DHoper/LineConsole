import type { ReactNode } from 'react';

/** 圖表的浮動說明：數值在前（粗體）、名稱在後；靠右半邊時往左翻，不會超出卡片 */
export function ChartTooltip({ x, width, children }: { x: number; width: number; children: ReactNode }) {
    const flip = x > width / 2;
    return (
        <div
            className="pointer-events-none absolute top-1 z-10 min-w-36 rounded-md border bg-popover px-3 py-2 text-xs shadow-md"
            style={flip ? { right: width - x + 12 } : { left: x + 12 }}
        >
            {children}
        </div>
    );
}

/** 說明裡每一列：一小段系列顏色的線當作識別，數值粗體、名稱次要 */
export function TooltipRow({ color, value, label }: { color?: string; value: string; label: string }) {
    return (
        <div className="flex items-center gap-2 py-0.5">
            {color ? <span className="h-0.5 w-3 rounded-full" style={{ background: color }} /> : <span className="w-3" />}
            <span className="font-semibold tabular-nums text-foreground">{value}</span>
            <span className="text-muted-foreground">{label}</span>
        </div>
    );
}
