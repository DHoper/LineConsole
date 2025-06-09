import { useState } from "react";
import { v4 as uuidv4 } from "uuid";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogTrigger,
} from "@/components/ui/dialog";

import type {
    MenuArea,
    MenuSize,
    MenuSize,
} from "../../types";

// 前端額外附加 id 用於編輯
type RichMenuAreaWithId = MenuArea & { id: string };

interface Preset {
  label: string;
  columns: number;
  rows: number;
  size: MenuSize;
}

const presets: Preset[] = [
    { label: "6欄 1列", columns: 6, rows: 1, size: "2500x843" },
    { label: "3欄 2列", columns: 3, rows: 2, size: "2500x1686" },
    { label: "2欄 2列", columns: 2, rows: 2, size: "2500x1686" },
    { label: "1欄 6列", columns: 1, rows: 6, size: "2500x1686" },
];

interface Props {
  trigger: React.ReactNode;
  onApply: (areas: RichMenuAreaWithId[], size: MenuSize) => void;
}

export function PresetLayoutDialog({ trigger, onApply }: Props) {
    const [open, setOpen] = useState(false);

    const generateAreas = (
        cols: number,
        rows: number,
        size: MenuSize
    ): RichMenuAreaWithId[] => {
        const [width, height] = size.split("x").map(Number);
        const cellWidth = Math.round(width / cols);
        const cellHeight = Math.round(height / rows);

        const areas: RichMenuAreaWithId[] = [];

        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < cols; col++) {
                areas.push({
                    id: uuidv4(),
                    bounds: {
                        x: col * cellWidth,
                        y: row * cellHeight,
                        width: cellWidth,
                        height: cellHeight,
                    },
                    action: { type: "none" },
                });
            }
        }

        return areas;
    };

    const handleSelectPreset = (preset: Preset) => {
        const areas = generateAreas(preset.columns, preset.rows, preset.size);
        onApply(areas, preset.size);
        setOpen(false);
    };

    const handleFreeDraw = () => {
    // 自由劃分畫布為 2500x1686
        onApply([], "2500x1686");
        setOpen(false);
    };

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>{trigger}</DialogTrigger>
            <DialogContent className="max-w-md space-y-4">
                <h2 className="text-xl font-bold">選擇版型</h2>

                <div className="grid grid-cols-1 gap-3">
                    {presets.map((preset) => (
                        <Button
                            key={preset.label}
                            variant="outline"
                            className="justify-start"
                            onClick={() => handleSelectPreset(preset)}
                        >
                            {preset.label}（{preset.columns}x{preset.rows}） - {preset.size}
                        </Button>
                    ))}
                </div>

                <div className="border-t pt-4">
                    <p className="text-sm text-gray-500 mb-2">不使用預設樣板？</p>
                    <Button variant="default" onClick={handleFreeDraw}>
            自由切割
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
