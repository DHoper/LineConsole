import type { RichMenuSchedule } from '../types';

export const STATUS_LABEL: Record<RichMenuSchedule['status'], string> = {
    Pending: '待執行',
    Active: '進行中',
    Completed: '已完成',
    Cancelled: '已取消',
    Failed: '失敗',
    Skipped: '已跳過',
};

const timeFormat = new Intl.DateTimeFormat('zh-TW', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false });

/** 排程時間顯示成瀏覽器的本地時間（後端存 UTC） */
export const formatTime = (iso: string) => timeFormat.format(new Date(iso));
