import type { MenuSize, MenuArea } from "./richMenuDto";

/** 建立 Rich Menu 並上傳圖片的請求資料 */
export interface CreateRichMenuRequest {
  selected: boolean;       // 是否為預設選單
  name: string;            // 圖文選單名稱
  chatBarText: string;     // Chat bar 顯示文字
  size: MenuSize;          // 圖文選單尺寸（寬度、高度）
  areas: MenuArea[];       // 點擊區域清單
  image: File;             // 圖片檔案（使用 multipart/form-data 傳送）
  scheduleStart?: string;  // 排程開始時間（ISO 字串，選填）
  scheduleEnd?: string;    // 排程結束時間（ISO 字串，選填）
}

/** 刪除指定 Rich Menu 的請求資料 */
export interface DeleteRichMenuRequest {
  richMenuId: string;      // 欲刪除的 Rich Menu ID
}
