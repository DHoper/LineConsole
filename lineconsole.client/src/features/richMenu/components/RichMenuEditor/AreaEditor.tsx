import { useState } from "react";
import { Rnd } from "react-rnd";

import { MenuArea } from "../../types";
import { AreaSettingsDialog } from "./AreaSettingsDialog";

interface Props {
    imageUrl: string;
    aspectRatio: number;
    areas: MenuArea[];
    setAreas: (areas: MenuArea[]) => void;
}

export function AreaEditor({ imageUrl, aspectRatio, areas, setAreas }: Props) {
    const [activeArea, setActiveArea] = useState<MenuArea | null>(null);

    const updateArea = (index: number, updated: Partial<MenuArea>) => {
        setAreas(
            areas.map((a, i) => (i === index ? { ...a, ...updated } : a))
        );
    };

    const deleteArea = (index: number) => {
        setAreas(areas.filter((_, i) => i !== index));
    };

    return (
        <div
            className="relative w-full border rounded-md overflow-hidden"
            style={{ aspectRatio }}
        >
            {/* 背景格線 */}
            <div
                className="absolute inset-0 bg-[length:20px_20px] bg-grid z-0"
                style={{
                    backgroundImage:
                        "linear-gradient(to right, #e5e7eb 1px, transparent 1px), linear-gradient(to bottom, #e5e7eb 1px, transparent 1px)",
                }}
            />
            <img
                src={imageUrl}
                alt="rich menu background"
                className="absolute inset-0 w-full h-full object-cover z-10"
            />

            {/* 區塊編輯 */}
            {areas.map((area, idx) => (
                <Rnd
                    key={`area-${area.bounds.x}-${area.bounds.y}-${idx}`}
                    size={{
                        width: area.bounds.width,
                        height: area.bounds.height,
                    }}
                    position={{
                        x: area.bounds.x,
                        y: area.bounds.y,
                    }}
                    onDragStop={(_, d) =>
                        updateArea(idx, {
                            bounds: {
                                ...area.bounds,
                                x: d.x,
                                y: d.y,
                            },
                        })
                    }
                    onResizeStop={(_, __, ref, ___, pos) =>
                        updateArea(idx, {
                            bounds: {
                                x: pos.x,
                                y: pos.y,
                                width: parseInt(ref.style.width, 10),
                                height: parseInt(ref.style.height, 10),
                            },
                        })
                    }
                    bounds="parent"
                    onClick={() => setActiveArea(area)}
                    className="absolute border-2 border-blue-500 bg-blue-200/40 cursor-move z-20"
                />
            ))}

            {/* 區塊設定 Dialog */}
            {activeArea && (
                <AreaSettingsDialog
                    area={activeArea}
                    onSave={(updated) => {
                        const index = areas.findIndex((a) => a === activeArea);
                        if (index !== -1) {
                            updateArea(index, updated);
                        }
                        setActiveArea(null);
                    }}
                    onDelete={() => {
                        const index = areas.findIndex((a) => a === activeArea);
                        if (index !== -1) {
                            deleteArea(index);
                        }
                        setActiveArea(null);
                    }}
                    onCancel={() => setActiveArea(null)}
                />
            )}
        </div>
    );
}
