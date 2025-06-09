import { LayoutTemplate } from "../types";

export const layoutTemplates: LayoutTemplate[] = [
    // === 大型 ===
    {
        id: "large-6x1",
        label: "六格橫排",
        size: "2500x1686",
        thumbnail: "/images/layouts/large-6x1.png",
        grid: [
            { x: 0, y: 0, w: 1, h: 1 },
            { x: 1, y: 0, w: 1, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
            { x: 3, y: 0, w: 1, h: 1 },
            { x: 4, y: 0, w: 1, h: 1 },
            { x: 5, y: 0, w: 1, h: 1 },
        ],
    },
    {
        id: "large-3x2",
        label: "三欄兩列",
        size: "2500x1686",
        thumbnail: "/images/layouts/large-3x2.png",
        grid: [
            { x: 0, y: 0, w: 1, h: 1 },
            { x: 1, y: 0, w: 1, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
            { x: 0, y: 1, w: 1, h: 1 },
            { x: 1, y: 1, w: 1, h: 1 },
            { x: 2, y: 1, w: 1, h: 1 },
        ],
    },
    {
        id: "large-half",
        label: "左右兩區",
        size: "2500x1686",
        thumbnail: "/images/layouts/large-half.png",
        grid: [
            { x: 0, y: 0, w: 3, h: 2 },
            { x: 3, y: 0, w: 3, h: 2 },
        ],
    },
    {
        id: "large-left-wide",
        label: "左大右小二格",
        size: "2500x1686",
        thumbnail: "/images/layouts/large-left-wide.png",
        grid: [
            { x: 0, y: 0, w: 4, h: 2 },
            { x: 4, y: 0, w: 2, h: 2 },
        ],
    },
    {
        id: "large-mixed",
        label: "上三下兩",
        size: "2500x1686",
        thumbnail: "/images/layouts/large-mixed.png",
        grid: [
            { x: 0, y: 0, w: 1, h: 1 },
            { x: 1, y: 0, w: 1, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
            { x: 0, y: 1, w: 2, h: 1 },
            { x: 2, y: 1, w: 1, h: 1 },
        ],
    },

    // === 小型 ===
    {
        id: "small-3x1",
        label: "三格橫排",
        size: "2500x843",
        thumbnail: "/images/layouts/small-3x1.png",
        grid: [
            { x: 0, y: 0, w: 1, h: 1 },
            { x: 1, y: 0, w: 1, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
        ],
    },
    {
        id: "small-2x1",
        label: "兩格橫排",
        size: "2500x843",
        thumbnail: "/images/layouts/small-2x1.png",
        grid: [
            { x: 0, y: 0, w: 2, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
        ],
    },
    {
        id: "small-left-large",
        label: "左大右小",
        size: "2500x843",
        thumbnail: "/images/layouts/small-left-large.png",
        grid: [
            { x: 0, y: 0, w: 2, h: 1 },
            { x: 2, y: 0, w: 1, h: 1 },
        ],
    },
    {
        id: "small-right-large",
        label: "右大左小",
        size: "2500x843",
        thumbnail: "/images/layouts/small-right-large.png",
        grid: [
            { x: 0, y: 0, w: 1, h: 1 },
            { x: 1, y: 0, w: 2, h: 1 },
        ],
    },
];
