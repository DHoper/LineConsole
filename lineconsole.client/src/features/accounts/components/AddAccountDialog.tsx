import { useState } from 'react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { useSystemInfo } from '@/features/system/api/systemAPI';

import { useCreateAccount } from '../api/accountsAPI';
import { useCurrentAccountStore } from '../stores/currentAccount';

const EMPTY = { channelName: '', channelId: '', channelSecret: '', channelAccessToken: '' };

/** 新增官方帳號：送出後後端會先拿 token 問 LINE，LINE 接受才存（加密存放） */
export default function AddAccountDialog({ trigger }: { trigger: React.ReactNode }) {
    const [open, setOpen] = useState(false);
    const [form, setForm] = useState(EMPTY);
    const create = useCreateAccount();
    const setAccountId = useCurrentAccountStore((s) => s.setAccountId);
    const { data: system } = useSystemInfo();
    const demo = system?.mode === 'Demo';

    const set = (key: keyof typeof EMPTY) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
        setForm((f) => ({ ...f, [key]: e.target.value }));

    const submit = () => {
        if (!form.channelId.trim() || !form.channelSecret.trim() || !form.channelAccessToken.trim()) {
            toast.warning('Channel ID、Channel Secret、Access Token 都要填');
            return;
        }
        create.mutate(
            { ...form, channelName: form.channelName.trim() || undefined },
            {
                onSuccess: (res) => {
                    if (res.data) {
                        setAccountId(res.data.id);
                        toast.success(`已綁定「${res.data.channelName}」`);
                    }
                    setForm(EMPTY);
                    setOpen(false);
                },
            },
        );
    };

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>{trigger}</DialogTrigger>
            <DialogContent className="max-w-lg">
                <DialogHeader>
                    <DialogTitle>新增 LINE 官方帳號</DialogTitle>
                </DialogHeader>

                <p className="text-sm text-muted-foreground">
                    在 LINE Developers 後台的 Messaging API 頁面可以找到這些值。送出後會先向 LINE 驗證 Access Token，
                    憑證加密存放，之後不會再顯示。
                </p>
                {demo && (
                    <p className="rounded-md border border-amber-200 bg-amber-50 p-2 text-xs text-amber-900">
                        展示模式：LINE 是模擬的，欄位隨便填就能綁定；Access Token 用 invalid 開頭，可以看 LINE 拒絕時的樣子。
                    </p>
                )}

                <div className="space-y-3">
                    <div className="space-y-1">
                        <Label htmlFor="channelName">顯示名稱（選填，不填就用 LINE 上的名稱）</Label>
                        <Input id="channelName" value={form.channelName} onChange={set('channelName')} />
                    </div>
                    <div className="space-y-1">
                        <Label htmlFor="channelId">Channel ID</Label>
                        <Input id="channelId" value={form.channelId} onChange={set('channelId')} />
                    </div>
                    <div className="space-y-1">
                        <Label htmlFor="channelSecret">Channel Secret</Label>
                        <Input id="channelSecret" type="password" value={form.channelSecret} onChange={set('channelSecret')} />
                    </div>
                    <div className="space-y-1">
                        <Label htmlFor="channelAccessToken">Channel Access Token（長期）</Label>
                        <Textarea id="channelAccessToken" rows={3} value={form.channelAccessToken} onChange={set('channelAccessToken')} />
                    </div>
                </div>

                <div className="flex justify-end gap-2 pt-2">
                    <Button variant="outline" onClick={() => setOpen(false)} disabled={create.isPending}>
                        取消
                    </Button>
                    <Button onClick={submit} disabled={create.isPending}>
                        {create.isPending ? '向 LINE 驗證中...' : '驗證並綁定'}
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
