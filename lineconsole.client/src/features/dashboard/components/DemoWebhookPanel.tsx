import { CircleCheck, CircleSlash, MessageSquare, Repeat, ShieldAlert, UserMinus, UserPlus } from 'lucide-react';
import { useState } from 'react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { sendForgedWebhook, type SimulateKind, useSimulateWebhook } from '@/features/friends/api/friendsAPI';

interface LogEntry {
    id: number;
    at: Date;
    tone: 'ok' | 'skip' | 'reject';
    text: string;
    detail?: string;
}

const ACTIONS: { kind: SimulateKind; label: string; icon: typeof UserPlus }[] = [
    { kind: 'follow', label: '新好友加入', icon: UserPlus },
    { kind: 'unfollow', label: '好友封鎖', icon: UserMinus },
    { kind: 'message', label: '好友傳訊息', icon: MessageSquare },
    { kind: 'redelivery', label: 'LINE 重送上一個事件', icon: Repeat },
];

const KIND_TEXT: Record<SimulateKind, string> = {
    follow: '新好友加入',
    unfollow: '好友封鎖',
    message: '好友傳訊息',
    redelivery: '重送事件',
};

const TONE = {
    ok: { icon: CircleCheck, className: 'text-emerald-600 dark:text-emerald-400' },
    skip: { icon: CircleSlash, className: 'text-muted-foreground' },
    reject: { icon: ShieldAlert, className: 'text-red-600 dark:text-red-400' },
};

const time = new Intl.DateTimeFormat('zh-TW', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });

/**
 * 展示模式才有：系統扮演 LINE 送 Webhook 進來。
 * 走的是正式流程（驗簽 → 去重 → 收件匣 → 背景處理），下面的紀錄直接顯示後端怎麼判斷：
 * 收下、重複略過（去重）、簽章不對拒絕（401）。
 */
export default function DemoWebhookPanel({ accountId }: { accountId: string }) {
    const simulate = useSimulateWebhook(accountId);
    const [log, setLog] = useState<LogEntry[]>([]);
    const [forging, setForging] = useState(false);

    const add = (entry: Omit<LogEntry, 'id' | 'at'>) =>
        setLog((prev) => [{ ...entry, id: Date.now() + Math.random(), at: new Date() }, ...prev].slice(0, 6));

    const run = (kind: SimulateKind) =>
        simulate.mutate(kind, {
            onSuccess: (res) => {
                const r = res.data;
                if (!r) return;
                if (r.duplicates > 0) {
                    add({ tone: 'skip', text: `${KIND_TEXT[kind]}：webhookEventId 已經收過，略過不重複處理`, detail: r.webhookEventId });
                } else {
                    add({ tone: 'ok', text: `${KIND_TEXT[kind]}：簽章正確，已存進收件匣，背景服務處理中`, detail: r.webhookEventId });
                }
            },
        });

    const forge = async () => {
        setForging(true);
        try {
            const r = await sendForgedWebhook(accountId);
            add(
                r.status === 401
                    ? { tone: 'reject', text: '偽造的請求：X-Line-Signature 驗證失敗，回 401 拒絕', detail: r.code }
                    : { tone: 'ok', text: `偽造的請求：伺服器回 ${r.status}（預期是 401）`, detail: r.code },
            );
        } finally {
            setForging(false);
        }
    };

    return (
        <Card className="gap-4">
            <CardHeader>
                <CardTitle className="text-base">模擬 LINE Webhook</CardTitle>
                <CardDescription>
                    展示模式沒有真的 LINE。按下按鈕由系統扮演 LINE 送出事件：一樣用 Channel Secret 簽章、經過驗簽與去重，存進收件匣後由背景服務更新好友資料。
                </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="flex flex-wrap gap-2">
                    {ACTIONS.map(({ kind, label, icon: Icon }) => (
                        <Button key={kind} variant="outline" size="sm" disabled={simulate.isPending} onClick={() => run(kind)}>
                            <Icon className="h-4 w-4" />
                            {label}
                        </Button>
                    ))}
                    <Button variant="outline" size="sm" disabled={forging} onClick={forge}>
                        <ShieldAlert className="h-4 w-4" />
                        送一個偽造簽章的請求
                    </Button>
                </div>

                {log.length > 0 && (
                    <ul className="divide-y rounded-md border text-sm" aria-live="polite">
                        {log.map((e) => {
                            const Icon = TONE[e.tone].icon;
                            return (
                                <li key={e.id} className="flex items-start gap-2 px-3 py-2">
                                    <Icon className={`mt-0.5 h-4 w-4 shrink-0 ${TONE[e.tone].className}`} aria-hidden />
                                    <div className="min-w-0 flex-1">
                                        <div>{e.text}</div>
                                        {e.detail && <div className="truncate font-mono text-xs text-muted-foreground">{e.detail}</div>}
                                    </div>
                                    <time className="shrink-0 text-xs tabular-nums text-muted-foreground">{time.format(e.at)}</time>
                                </li>
                            );
                        })}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
}
