import { useEffect, useState } from 'react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

import { useCreateSchedule } from '../api/useRichMenuMutation';

/** <input type="datetime-local"> 用的本地時間字串（YYYY-MM-DDTHH:mm） */
const toLocalInput = (d: Date) => {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const defaultWindow = () => {
    const start = new Date(Date.now() + 5 * 60_000);
    start.setSeconds(0, 0);
    return { start: toLocalInput(start), end: toLocalInput(new Date(start.getTime() + 60 * 60_000)) };
};

interface Props {
    accountId: string;
    menu: { richMenuId: string; name: string } | null;
    onClose: () => void;
}

/** 設定排程：開始時把這個選單設成預設，結束時自動還原成原本的預設選單 */
export default function ScheduleDialog({ accountId, menu, onClose }: Props) {
    const [range, setRange] = useState(defaultWindow);
    const create = useCreateSchedule(accountId);

    useEffect(() => {
        if (menu) setRange(defaultWindow());
    }, [menu]);

    const submit = () => {
        if (!menu) return;
        const start = new Date(range.start);
        const end = new Date(range.end);
        if (!(end > start)) return toast.error('結束時間要晚於開始時間');
        if (end <= new Date()) return toast.error('結束時間已經過了');

        create.mutate(
            { richMenuId: menu.richMenuId, startTime: start.toISOString(), endTime: end.toISOString() },
            {
                onSuccess: () => {
                    toast.success('排程已建立，時間到會自動切換');
                    onClose();
                },
            },
        );
    };

    return (
        <Dialog open={!!menu} onOpenChange={(o) => !o && onClose()}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>排程：{menu?.name}</DialogTitle>
                </DialogHeader>
                <p className="text-sm text-muted-foreground">
                    開始時把這個選單設成預設；結束時自動還原成排程前的預設選單（如果中途有人手動換過，就不會去動它）。
                </p>
                <div className="grid grid-cols-2 gap-3">
                    <div className="space-y-1">
                        <Label htmlFor="schedule-start">開始</Label>
                        <Input id="schedule-start" type="datetime-local" value={range.start}
                            onChange={(e) => setRange((r) => ({ ...r, start: e.target.value }))} />
                    </div>
                    <div className="space-y-1">
                        <Label htmlFor="schedule-end">結束</Label>
                        <Input id="schedule-end" type="datetime-local" value={range.end}
                            onChange={(e) => setRange((r) => ({ ...r, end: e.target.value }))} />
                    </div>
                </div>
                <div className="flex justify-end gap-2 pt-2">
                    <Button variant="outline" onClick={onClose} disabled={create.isPending}>取消</Button>
                    <Button onClick={submit} disabled={create.isPending}>{create.isPending ? '建立中...' : '建立排程'}</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
