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
    MenuSize
} from "../../types";

// 前端額外附加 id 用於編輯
type RichMenuAreaWithId = MenuArea & { id: string };

interface Preset {
  label: string;
  columns: number;
  rows: number;
  size: MenuSize;
}

// LINE 圖文選單常用的兩種尺寸
const FULL: MenuSize = { width: 2500, height: 1686 };
const HALF: MenuSize = { width: 2500, height: 843 };

const presets: Preset[] = [
    { label: "6欄 1列", columns: 6, rows: 1, size: HALF },
    { label: "3欄 2列", columns: 3, rows: 2, size: FULL },
    { label: "2欄 2列", columns: 2, rows: 2, size: FULL },
    { label: "1欄 6列", columns: 1, rows: 6, size: FULL },
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
        const { width, height } = size;
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
        // 自由劃分：畫布 2500x1686
        onApply([], FULL);
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
                            {preset.label}（{preset.columns}x{preset.rows}） - {preset.size.width}x{preset.size.height}
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
