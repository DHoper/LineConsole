import {
    Bot,
    Building2,
    FileText,
    Home,
    Layout,
    type LucideIcon,
    Mail,
    MessageCircle,
    Puzzle,
    Send,
    Tags,
    Users,
} from 'lucide-react';

export interface MenuItem {
    label: string;
    icon: LucideIcon;
    /** 已完成的功能才有路徑 */
    path?: string;
    /** 規劃中：顯示在選單上讓人看到產品藍圖，但不能點、沒有空頁面 */
    planned?: boolean;
    /** 規劃中的項目，滑過去顯示的說明 */
    note?: string;
}

/** 已完成的功能 */
export const mainMenu: MenuItem[] = [
    { label: '官方帳號', path: '/accounts', icon: Building2 },
    { label: '圖文選單', path: '/rich-menu', icon: Layout },
];

/** 規劃中的功能（產品藍圖） */
export const plannedMenu: MenuItem[] = [
    { label: '概覽', icon: Home, planned: true, note: '好友數趨勢、推播成效、排程狀態' },
    { label: '好友管理', icon: Users, planned: true, note: '好友列表、篩選、封鎖名單（接 LINE Webhook）' },
    { label: '推播訊息', icon: Send, planned: true, note: '排程推播、發送紀錄、失敗重試' },
    { label: '標籤管理', icon: Tags, planned: true, note: '依標籤分眾推播' },
    { label: '對話管理', icon: MessageCircle, planned: true, note: '一對一聊天紀錄' },
    { label: '發布文章', icon: FileText, planned: true, note: 'LINE VOOM 貼文' },
    { label: '聊天機器人', icon: Bot, planned: true, note: '關鍵字回覆、常見問題' },
    { label: '訊息工具', icon: Mail, planned: true, note: '訊息範本、追蹤連結' },
    { label: '進階功能', icon: Puzzle, planned: true, note: '集點卡、優惠券、抽獎、即時投票' },
];
