import { useEffect, useRef, useState } from 'react';

/** 系列顏色（index.css 的 --viz-*，亮色／暗色各自驗證過色盲辨識度與對比） */
export const FOLLOW_COLOR = 'var(--viz-follow)';
export const UNFOLLOW_COLOR = 'var(--viz-unfollow)';

/** 量容器寬度（SVG 圖表跟著版面縮放，不寫死寬度） */
export function useElementWidth<T extends HTMLElement>() {
    const ref = useRef<T>(null);
    const [width, setWidth] = useState(0);
    useEffect(() => {
        const el = ref.current;
        if (!el) return;
        const observer = new ResizeObserver(([entry]) => setWidth(Math.floor(entry.contentRect.width)));
        observer.observe(el);
        return () => observer.disconnect();
    }, []);
    return [ref, width] as const;
}

/** 刻度間距取 1、2、5 的倍數（0 / 50 / 100 / 150 這種乾淨的數字） */
export function niceStep(max: number, count = 4) {
    if (max <= 0) return 1;
    const raw = max / count;
    const magnitude = 10 ** Math.floor(Math.log10(raw));
    const step = [1, 2, 5, 10].map((s) => s * magnitude).find((s) => s >= raw) ?? 10 * magnitude;
    return Math.max(1, step);
}

export function ticks(min: number, max: number, step: number) {
    const out: number[] = [];
    for (let v = Math.floor(min / step) * step; v <= max + 1e-9; v += step) out.push(v);
    return out;
}

/** 長條：資料端 4px 圓角、基準線那端是直角。up=true 往上長，false 往下長 */
export function barPath(x: number, width: number, baseY: number, endY: number) {
    const h = Math.abs(endY - baseY);
    if (h < 0.5) return '';
    const r = Math.min(4, width / 2, h);
    if (endY < baseY) {
        return `M${x},${baseY}V${endY + r}Q${x},${endY} ${x + r},${endY}H${x + width - r}Q${x + width},${endY} ${x + width},${endY + r}V${baseY}Z`;
    }
    return `M${x},${baseY}V${endY - r}Q${x},${endY} ${x + r},${endY}H${x + width - r}Q${x + width},${endY} ${x + width},${endY - r}V${baseY}Z`;
}

/** 鍵盤操作圖表：左右鍵換天、Home/End 跳到頭尾 */
export function nextIndex(key: string, current: number | null, length: number): number | null {
    const at = current ?? length - 1;
    switch (key) {
    case 'ArrowLeft': return Math.max(0, at - 1);
    case 'ArrowRight': return Math.min(length - 1, at + 1);
    case 'Home': return 0;
    case 'End': return length - 1;
    default: return null;
    }
}
