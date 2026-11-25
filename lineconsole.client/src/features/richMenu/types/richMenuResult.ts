import type { MenuArea,MenuSize } from "./richMenu";

/** 回傳單一 Rich Menu 詳細資料 */
export interface RichMenuResult {
  richMenuId: string;       // Rich Menu ID
  name: string;             // 選單名稱
  chatBarText: string;      // Chat bar 提示文字
  selected: boolean;        // 是否為預設
  size: MenuSize;           // 尺寸資訊
  areas: MenuArea[];        // 點擊區域清單
}

/** 圖文選單排程（時間是 UTC 的 ISO 字串） */
export interface RichMenuSchedule {
  id: string;
  richMenuId: string;       // LINE 的圖文選單 ID
  richMenuName: string;
  startTime: string;
  endTime: string;
  status: "Pending" | "Active" | "Completed" | "Cancelled" | "Failed" | "Skipped";
  activatedAt: string | null;
  completedAt: string | null;
  lastError: string | null;
}

export interface CreateRichMenuScheduleInput {
  richMenuId: string;
  startTime: string;
  endTime: string;
}

/** 回傳 Rich Menu ID 結果 */
export interface RichMenuIdResult {
  richMenuId: string;       // Rich Menu ID
}

/** 回傳 Rich Menu 清單結果 */
export interface RichMenuListResult {
  richMenus: RichMenuResult[];  // Rich Menu 陣列
}

/** 回傳 Rich Menu（含圖片 base64 預覽） */
export interface RichMenuWithImageResult {
  richMenuId: string;       // Rich Menu ID
  name: string;             // 選單名稱
  width: number;            // 寬度（展開後尺寸）
  height: number;           // 高度
  selected: boolean;        // 是否為預設
  chatBarText: string;      // 提示文字
  imageBase64: string;      // 圖片預覽 base64 字串
}
