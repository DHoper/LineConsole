import { CalendarClock } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { cn } from '@/libs/ui/utils';

import { useCancelSchedule } from '../api/useRichMenuMutation';
import type { RichMenuSchedule } from '../types';
import { formatTime, STATUS_LABEL } from '../utils/schedule';

const STATUS_STYLE: Record<RichMenuSchedule['status'], string> = {
    Pending: 'bg-blue-50 text-blue-700 border-blue-200',
    Active: 'bg-green-50 text-green-700 border-green-200',
    Completed: 'bg-gray-50 text-gray-600 border-gray-200',
    Cancelled: 'bg-gray-50 text-gray-400 border-gray-200',
    Failed: 'bg-red-50 text-red-700 border-red-200',
    Skipped: 'bg-amber-50 text-amber-700 border-amber-200',
};

/** 排程清單：背景服務會改變狀態，所以每 10 秒重抓一次 */
export default function SchedulesPanel({ accountId, schedules }: { accountId: string; schedules: RichMenuSchedule[] }) {
    const cancel = useCancelSchedule(accountId);

    return (
        <section className="space-y-3">
            <h2 className="text-lg font-semibold flex items-center gap-2">
                <CalendarClock className="w-5 h-5" />
                排程
            </h2>
            {schedules.length === 0 ? (
                <p className="text-sm text-muted-foreground border rounded bg-white p-4">
                    還沒有排程。在選單卡片的「⋮」選「設定排程」，可以讓選單在指定時段自動上線、結束後自動還原。
                </p>
            ) : (
                <div className="border rounded-lg bg-white overflow-hidden">
                    <table className="w-full text-sm">
                        <thead className="bg-muted/50 text-left text-muted-foreground">
                            <tr>
                                <th className="px-4 py-2 font-medium">選單</th>
                                <th className="px-4 py-2 font-medium">時段</th>
                                <th className="px-4 py-2 font-medium">狀態</th>
                                <th className="px-4 py-2" />
                            </tr>
                        </thead>
                        <tbody>
                            {schedules.map((s) => (
                                <tr key={s.id} className="border-t">
                                    <td className="px-4 py-2">{s.richMenuName}</td>
                                    <td className="px-4 py-2 tabular-nums">
                                        {formatTime(s.startTime)} ～ {formatTime(s.endTime)}
                                    </td>
                                    <td className="px-4 py-2">
                                        <span title={s.lastError ?? undefined}
                                            className={cn('inline-block rounded border px-2 py-0.5 text-xs', STATUS_STYLE[s.status])}>
                                            {STATUS_LABEL[s.status]}
                                        </span>
                                        {s.lastError && <p className="text-xs text-muted-foreground mt-1">{s.lastError}</p>}
                                    </td>
                                    <td className="px-4 py-2 text-right">
                                        {s.status === 'Pending' && (
                                            <Button size="sm" variant="ghost" disabled={cancel.isPending}
                                                onClick={() => cancel.mutate(s.id, { onSuccess: () => toast.success('排程已取消') })}>
                                                取消
                                            </Button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </section>
    );
}
