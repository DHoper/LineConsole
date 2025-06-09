/** 建立與驗證 Rich Menu 結構資料（傳送至後端或 LINE API） */
export interface RichMenu {
  size: MenuSize;                // 選單尺寸
  selected: boolean;            // 是否為預設選單
  name: string;                 // 選單名稱
  chatBarText: string;          // Chat bar 提示文字
  areas: MenuArea[];            // 點擊區域清單
  startTime?: string;           // 排程起始時間（ISO 8601 格式）
  endTime?: string;             // 排程結束時間（ISO 8601 格式）
}

/** Rich Menu 尺寸資訊 */
export interface MenuSize {
  width: number;                // 寬度（px）
  height: number;               // 高度（px）
}

/** 點擊區域座標範圍 */
export interface MenuBounds {
  x: number;                    // 左上角 X 座標
  y: number;                    // 左上角 Y 座標
  width: number;                // 區域寬度
  height: number;               // 區域高度
}

/** 點擊行為類型（對應 LINE Messaging API） */
export type LineRichMenuActionType =
  | "postback"
  | "uri"
  | "message"
  | "richmenuswitch"
  | "datetimepicker"
  | "camera"
  | "cameraroll"
  | "location"
  | "clipboard"
  | "none";

/** 點擊行為設定 */
export interface MenuAction {
  type: LineRichMenuActionType; // 行為類型
  text?: string;                // message 類型對應的訊息文字
  data?: string;                // postback 類型對應的資料
  uri?: string;                 // uri 類型對應的網址
  richMenuAliasId?: string;     // richmenuswitch 對應的 alias ID
  dateTime?: string;            // datetimepicker 類型對應的 ISO 字串
}

/** 點擊區塊設定 */
export interface MenuArea {
  bounds: MenuBounds;           // 點擊範圍
  action: MenuAction;           // 點擊後執行的行為
}
