import { useState } from 'react';

import type { DailyFriendStat } from '@/features/friends/api/friendsAPI';
import { formatDay, formatNumber, formatSigned } from '@/features/friends/utils/format';

import { ChartTooltip, TooltipRow } from './ChartTooltip';
import { FOLLOW_COLOR, nextIndex, niceStep, ticks, useElementWidth } from './chartUtils';

const HEIGHT = 240;
const M = { top: 16, right: 44, bottom: 28, left: 44 };
const SERIES = FOLLOW_COLOR;

/**
 * 近 30 天的好友人數（單一系列：標題就說明了畫的是什麼，不放圖例）。
 * 滑鼠或鍵盤（左右鍵）移動時，垂直參考線對齊最近的一天，說明框列出當天的人數、新增、封鎖。
 */
export default function FriendTrendChart({ daily }: { daily: DailyFriendStat[] }) {
    const [ref, width] = useElementWidth<HTMLDivElement>();
    const [active, setActive] = useState<number | null>(null);
    const n = daily.length;

    const plotW = Math.max(0, width - M.left - M.right);
    const plotH = HEIGHT - M.top - M.bottom;
    const max = Math.max(1, ...daily.map((d) => d.following));
    const step = niceStep(max);
    const yTicks = ticks(0, Math.ceil(max / step) * step, step);
    const yMax = yTicks[yTicks.length - 1];
    const x = (i: number) => M.left + (n <= 1 ? plotW / 2 : (i * plotW) / (n - 1));
    const y = (v: number) => M.top + plotH * (1 - v / yMax);
    const baseY = y(0);

    const line = daily.map((d, i) => `${i ? 'L' : 'M'}${x(i)},${y(d.following)}`).join('');
    const area = n ? `${line}L${x(n - 1)},${baseY}L${x(0)},${baseY}Z` : '';
    const xLabels = n ? [0, 7, 14, 21, n - 1].filter((i) => i < n) : [];
    const last = daily[n - 1];
    const hovered = active !== null ? daily[active] : null;

    const pick = (clientX: number, rect: DOMRect) => {
        if (n === 0 || plotW <= 0) return;
        const i = Math.round(((clientX - rect.left - M.left) / plotW) * (n - 1));
        setActive(Math.min(n - 1, Math.max(0, i)));
    };

    return (
        <div ref={ref} className="relative w-full" style={{ height: HEIGHT }}>
            {width > 0 && n > 0 && (
                <svg
                    width={width}
                    height={HEIGHT}
                    role="img"
                    tabIndex={0}
                    aria-label={`近 ${n} 天好友人數，從 ${daily[0].following} 位到 ${last.following} 位。用左右鍵查看每一天。`}
                    className="outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-md"
                    onPointerMove={(e) => pick(e.clientX, e.currentTarget.getBoundingClientRect())}
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
                    {/* 格線、刻度 */}
                    {yTicks.map((t) => (
                        <g key={t}>
                            <line x1={M.left} x2={M.left + plotW} y1={y(t)} y2={y(t)} stroke={t === 0 ? 'var(--viz-axis)' : 'var(--viz-grid)'} strokeWidth={1} />
                            <text x={M.left - 8} y={y(t)} dy="0.32em" textAnchor="end" className="fill-muted-foreground text-[11px] tabular-nums">
                                {formatNumber(t)}
                            </text>
                        </g>
                    ))}
                    {xLabels.map((i) => (
                        <text key={i} x={x(i)} y={HEIGHT - 8} textAnchor={i === 0 ? 'start' : i === n - 1 ? 'end' : 'middle'} className="fill-muted-foreground text-[11px] tabular-nums">
                            {formatDay(daily[i].date)}
                        </text>
                    ))}

                    {/* 資料：淡淡的面積＋2px 線 */}
                    <path d={area} fill={SERIES} fillOpacity={0.1} />
                    <path d={line} fill="none" stroke={SERIES} strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />

                    {/* 終點：圓點＋數值（只標這一個） */}
                    <circle cx={x(n - 1)} cy={y(last.following)} r={4} fill={SERIES} stroke="var(--card)" strokeWidth={2} />
                    <text x={x(n - 1) + 8} y={y(last.following)} dy="0.32em" className="fill-foreground text-xs font-semibold">
                        {formatNumber(last.following)}
                    </text>

                    {/* 參考線 */}
                    {active !== null && hovered && (
                        <g pointerEvents="none">
                            <line x1={x(active)} x2={x(active)} y1={M.top} y2={baseY} stroke="var(--viz-axis)" strokeWidth={1} />
                            <circle cx={x(active)} cy={y(hovered.following)} r={4} fill={SERIES} stroke="var(--card)" strokeWidth={2} />
                        </g>
                    )}
                </svg>
            )}
            {active !== null && hovered && (
                <ChartTooltip x={x(active)} width={width}>
                    <div className="mb-1 text-muted-foreground">{hovered.date}</div>
                    <TooltipRow color={SERIES} value={`${formatNumber(hovered.following)} 位`} label="好友人數" />
                    <TooltipRow value={formatSigned(hovered.follows - hovered.unfollows)} label={`當天淨增（新增 ${hovered.follows}、封鎖 ${hovered.unfollows}）`} />
                </ChartTooltip>
            )}
        </div>
    );
}
