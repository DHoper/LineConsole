const dateTime = new Intl.DateTimeFormat('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false });
const shortDate = new Intl.DateTimeFormat('zh-TW', { month: 'numeric', day: 'numeric' });
const number = new Intl.NumberFormat('zh-TW');

/** 後端存 UTC，顯示成瀏覽器的本地時間 */
export const formatDateTime = (iso: string | null) => (iso ? dateTime.format(new Date(iso)) : '—');

/** "2026-11-25" → "11/25"（日期本身已經是台北時間的日期，不再換時區） */
export const formatDay = (date: string) => {
    const [y, m, d] = date.split('-').map(Number);
    return shortDate.format(new Date(y, m - 1, d));
};

export const formatNumber = (n: number) => number.format(n);

export const formatSigned = (n: number) => (n > 0 ? `+${number.format(n)}` : n < 0 ? `−${number.format(-n)}` : '0');
