import { v4 as uuidv4 } from "uuid";

import { Area, LayoutTemplate } from "../types";

// 預設網格為 6x2（以 2500x1686 為例）
const DEFAULT_GRID = {
    cols: 6,
    rows: 2,
};

export function generateAreasFromTemplate(template: LayoutTemplate): Area[] {
    const [width, height] = template.size.split("x").map(Number);
    const { cols, rows } = DEFAULT_GRID;

    const cellWidth = width / cols;
    const cellHeight = height / rows;

    return template.grid.map((item) => ({
        id: uuidv4(),
        bounds: {
            x: item.x * cellWidth,
            y: item.y * cellHeight,
            width: item.w * cellWidth,
            height: item.h * cellHeight,
        },
        action: { type: "none" },
    }));
}
