import { X } from "lucide-react";
import { MouseEvent, useRef, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import type { MenuArea, MenuBounds, MenuSize } from "../../types";

interface Props {
    imageUrl: string;
    areas: MenuArea[];
    onAreaAdd: (area: MenuArea) => void;
    onAreaRemove: (index: number) => void;
    onAreaClick?: (area: MenuArea) => void;
    onOverlap?: () => void;
    size: MenuSize; 
}

export function ImageGridEditor({
    imageUrl,
    areas,
    onAreaAdd,
    onAreaRemove,
    onAreaClick,
    onOverlap,
    size,
}: Props) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [start, setStart] = useState<{ x: number; y: number } | null>(null);
    const [end, setEnd] = useState<{ x: number; y: number } | null>(null);

    const height = size.height;
    const width = size.width;

    const getGridPosition = (e: MouseEvent<HTMLDivElement>) => {
        const rect = containerRef.current?.getBoundingClientRect();
        if (!rect) return null;
        const x = Math.floor(((e.clientX - rect.left) / rect.width) * width);
        const y = Math.floor(((e.clientY - rect.top) / rect.height) * height);
        return { x, y };
    };

    const handleMouseDown = (e: MouseEvent<HTMLDivElement>) => {
        const pos = getGridPosition(e);
        if (pos) setStart(pos);
    };

    const handleMouseMove = (e: MouseEvent<HTMLDivElement>) => {
        if (!start) return;
        const pos = getGridPosition(e);
        if (pos) setEnd(pos);
    };

    const handleMouseUp = () => {
        if (start && end) {
            const x = Math.min(start.x, end.x);
            const y = Math.min(start.y, end.y);
            const widthPx = Math.abs(start.x - end.x);
            const heightPx = Math.abs(start.y - end.y);

            if (widthPx < 20 || heightPx < 20) {
                setStart(null);
                setEnd(null);
                return;
            }

            const newBounds: MenuBounds = {
                x,
                y,
                width: widthPx,
                height: heightPx,
            };

            const isOverlap = areas.some((a) => {
                const b = a.bounds;
                return !(
                    newBounds.x + newBounds.width <= b.x ||
                    newBounds.x >= b.x + b.width ||
                    newBounds.y + newBounds.height <= b.y ||
                    newBounds.y >= b.y + b.height
                );
            });

            if (isOverlap) {
                onOverlap?.();
            } else {
                onAreaAdd({
                    id: uuidv4(),
                    bounds: newBounds,
                    action: { type: "none" },
                });
            }
        }

        setStart(null);
        setEnd(null);
    };

    const getAreaStyle = (area: MenuArea) => ({
        left: `${(area.bounds.x / width) * 100}%`,
        top: `${(area.bounds.y / height) * 100}%`,
        width: `${(area.bounds.width / width) * 100}%`,
        height: `${(area.bounds.height / height) * 100}%`,
    });

    return (
        <div
            ref={containerRef}
            className="relative border rounded-md bg-white overflow-hidden"
            style={{ aspectRatio: `${width} / ${height}` }}
            onMouseDown={handleMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
        >
            <img
                src={imageUrl}
                alt="rich menu"
                className="absolute w-full h-full object-cover pointer-events-none select-none"
            />

            {/* 拖曳中的預覽區塊 */}
            {start && end && (() => {
                const x = Math.min(start.x, end.x);
                const y = Math.min(start.y, end.y);
                const w = Math.abs(start.x - end.x);
                const h = Math.abs(start.y - end.y);

                return (
                    <div
                        className="absolute bg-blue-400/30 border-2 border-blue-600 z-10 pointer-events-none"
                        style={{
                            left: `${(x / width) * 100}%`,
                            top: `${(y / height) * 100}%`,
                            width: `${(w / width) * 100}%`,
                            height: `${(h / height) * 100}%`,
                        }}
                    />
                );
            })()}

            {/* 既有區塊渲染 */}
            {areas.map((area) => (
                <div
                    key={`${area.bounds.x}-${area.bounds.y}-${area.bounds.width}-${area.bounds.height}`}
                    className="absolute border-2 border-red-500 bg-red-500/20 z-20 cursor-pointer"
                    style={getAreaStyle(area)}
                    onClick={(e) => {
                        e.stopPropagation();
                        if ("id" in area) onAreaClick?.(area as MenuArea & { id: string });
                    }}
                >
                    <button
                        className="absolute top-0 right-0 bg-white text-red-600 rounded-bl px-1 py-0.5"
                        onClick={(e) => {
                            e.stopPropagation();
                            if ("id" in area) onAreaRemove((area as { id: string }).id);
                        }}
                    >
                        <X className="w-3 h-3" />
                    </button>
                </div>
            ))}
        </div>
    );
}
