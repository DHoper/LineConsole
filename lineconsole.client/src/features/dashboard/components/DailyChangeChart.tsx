import { useState } from 'react';

import type { DailyFriendStat } from '@/features/friends/api/friendsAPI';
import { formatDay, formatNumber, formatSigned } from '@/features/friends/utils/format';

import { ChartTooltip, TooltipRow } from './ChartTooltip';
import { barPath, FOLLOW_COLOR, nextIndex, niceStep, ticks, UNFOLLOW_COLOR, useElementWidth } from './chartUtils';

const HEIGHT = 220;
const M = { top: 20, right: 12, bottom: 28, left: 44 };

/**
 * 每天的新增（基準線以上）和封鎖（基準線以下）：方向本身就是意義，所以用上下對稱的長條，而不是兩組並排。
 * 每一天整欄都是滑鼠／鍵盤的目標；只在新增最多的那天標數字。
 */
export default function DailyChangeChart({ daily }: { daily: DailyFriendStat[] }) {
    const [ref, width] = useElementWidth<HTMLDivElement>();
    const [active, setActive] = useState<number | null>(null);
    const n = daily.length;

    const plotW = Math.max(0, width - M.left - M.right);
    const plotH = HEIGHT - M.top - M.bottom;
    const up = Math.max(1, ...daily.map((d) => d.follows));
    const down = Math.max(0, ...daily.map((d) => d.unfollows));
    const step = niceStep(up + down, 4);
    const top = Math.ceil(up / step) * step;
    const bottom = -Math.ceil(down / step) * step;
    const yTicks = ticks(bottom, top, step);
    const y = (v: number) => M.top + (plotH * (top - v)) / (top - bottom || 1);
    const baseY = y(0);

    const band = n ? plotW / n : 0;
    const barW = Math.max(2, Math.min(24, band * 0.62));
    const bandX = (i: number) => M.left + i * band;
    const barX = (i: number) => bandX(i) + (band - barW) / 2;
    const peak = daily.reduce((best, d, i) => (d.follows > daily[best].follows ? i : best), 0);
    const xLabels = n ? [0, 7, 14, 21, n - 1].filter((i) => i < n) : [];
    const hovered = active !== null ? daily[active] : null;

    return (
        <div ref={ref} className="relative w-full" style={{ height: HEIGHT }}>
            {width > 0 && n > 0 && (
                <svg
                    width={width}
                    height={HEIGHT}
                    role="img"
                    tabIndex={0}
                    aria-label={`近 ${n} 天每日新增與封鎖。新增最多的是 ${daily[peak].date}，${daily[peak].follows} 位。用左右鍵查看每一天。`}
                    className="outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-md"
                    onPointerLeave={() => setActive(null)}
                    onFocus={() => setActive(n - 1)}
                    onBlur={() => setActive(null)}
                    onKeyDown={(e) => {
                        const next = nextIndex(e.key, active, n);
                        if (next !== null) {
                            e.preventDefault();
                            setActive(next);
                        }
                    }}
                >
                    {yTicks.map((t) => (
                        <g key={t}>
                            <line x1={M.left} x2={M.left + plotW} y1={y(t)} y2={y(t)} stroke={t === 0 ? 'var(--viz-axis)' : 'var(--viz-grid)'} strokeWidth={1} />
                            <text x={M.left - 8} y={y(t)} dy="0.32em" textAnchor="end" className="fill-muted-foreground text-[11px] tabular-nums">
                                {t < 0 ? `−${formatNumber(-t)}` : formatNumber(t)}
                            </text>
                        </g>
                    ))}
                    {xLabels.map((i) => (
                        <text key={i} x={bandX(i) + band / 2} y={HEIGHT - 8} textAnchor={i === 0 ? 'start' : i === n - 1 ? 'end' : 'middle'} className="fill-muted-foreground text-[11px] tabular-nums">
                            {formatDay(daily[i].date)}
                        </text>
                    ))}

                    {active !== null && <rect x={bandX(active)} y={M.top} width={band} height={plotH} className="fill-muted" opacity={0.6} pointerEvents="none" />}

                    {daily.map((d, i) => (
                        <g key={d.date}>
                            <path d={barPath(barX(i), barW, baseY, y(d.follows))} fill={FOLLOW_COLOR} />
                            <path d={barPath(barX(i), barW, baseY, y(-d.unfollows))} fill={UNFOLLOW_COLOR} />
                            {/* 整欄都是目標，比長條本身大 */}
                            <rect x={bandX(i)} y={M.top} width={band} height={plotH} fill="transparent" onPointerEnter={() => setActive(i)} />
                        </g>
                    ))}

                    <text x={barX(peak) + barW / 2} y={y(daily[peak].follows) - 6} textAnchor="middle" className="fill-foreground text-xs font-semibold" pointerEvents="none">
                        +{daily[peak].follows}
                    </text>
                </svg>
            )}
            {active !== null && hovered && (
                <ChartTooltip x={bandX(active) + band / 2} width={width}>
                    <div className="mb-1 text-muted-foreground">{hovered.date}</div>
                    <TooltipRow color={FOLLOW_COLOR} value={`+${hovered.follows}`} label="新增好友" />
                    <TooltipRow color={UNFOLLOW_COLOR} value={hovered.unfollows ? `−${hovered.unfollows}` : '0'} label="封鎖" />
                    <TooltipRow value={formatSigned(hovered.follows - hovered.unfollows)} label="淨增" />
                </ChartTooltip>
            )}
        </div>
    );
}
