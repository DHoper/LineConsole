import type { MenuArea,MenuSize } from "./richMenu";

/** 建立 Rich Menu 並上傳圖片的請求資料 */
export interface CreateRichMenuInput {
  selected: boolean;       // 是否為預設選單
  name: string;            // 圖文選單名稱
  chatBarText: string;     // Chat bar 顯示文字
  size: MenuSize;          // 圖文選單尺寸（寬度、高度）
  areas: MenuArea[];       // 點擊區域清單
  image: File;             // 圖片檔案（使用 multipart/form-data 傳送）
}

/** 刪除指定 Rich Menu 的請求資料 */
export interface DeleteRichMenuInput {
  richMenuId: string;      // 欲刪除的 Rich Menu ID
}
