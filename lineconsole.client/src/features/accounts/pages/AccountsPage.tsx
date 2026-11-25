import { Check, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';

import { type OfficialAccount, useAccounts, useDeleteAccount } from '../api/accountsAPI';
import AddAccountDialog from '../components/AddAccountDialog';
import { useCurrentAccount, useCurrentAccountStore } from '../stores/currentAccount';

const dateFormat = new Intl.DateTimeFormat('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' });

/** 官方帳號管理：列表、新增（向 LINE 驗證）、切換、解除綁定 */
export default function AccountsPage() {
    const { data: accounts = [], isLoading } = useAccounts();
    const current = useCurrentAccount();
    const setAccountId = useCurrentAccountStore((s) => s.setAccountId);
    const remove = useDeleteAccount();
    const [confirming, setConfirming] = useState<OfficialAccount | null>(null);

    const addButton = (
        <Button>
            <Plus className="w-4 h-4 mr-2" />
            新增官方帳號
        </Button>
    );

    return (
        <div className="p-6 space-y-6 max-w-5xl mx-auto">
            <div className="flex items-start justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold">LINE 官方帳號</h1>
                    <p className="text-sm text-muted-foreground mt-1">
                        一個後台可以管理多個官方帳號；上方的下拉選單切換目前操作的帳號。
                    </p>
                </div>
                <AddAccountDialog trigger={addButton} />
            </div>

            {isLoading ? (
                <p className="text-sm text-muted-foreground">載入中...</p>
            ) : accounts.length === 0 ? (
                <div className="text-center py-16 border rounded-lg bg-white space-y-4">
                    <p className="text-muted-foreground">還沒有綁定 LINE 官方帳號</p>
                    <AddAccountDialog trigger={addButton} />
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2">
                    {accounts.map((a) => {
                        const isCurrent = a.id === current?.id;
                        return (
                            <div key={a.id} className="border rounded-lg bg-white p-5 space-y-3 shadow-sm">
                                <div className="flex items-start justify-between gap-2">
                                    <div className="min-w-0">
                                        <h2 className="font-semibold truncate">{a.channelName}</h2>
                                        <p className="text-sm text-muted-foreground">{a.basicId ?? '（沒有 @ID）'}</p>
                                    </div>
                                    {isCurrent && <Badge>目前使用中</Badge>}
                                </div>
                                <dl className="text-xs text-muted-foreground grid grid-cols-[auto_1fr] gap-x-3 gap-y-1">
                                    <dt>Channel ID</dt>
                                    <dd className="font-mono">{a.channelId}</dd>
                                    <dt>綁定日期</dt>
                                    <dd>{dateFormat.format(new Date(a.createdAt))}</dd>
                                </dl>
                                <div className="flex gap-2 pt-1">
                                    <Button size="sm" variant="outline" disabled={isCurrent} onClick={() => setAccountId(a.id)}>
                                        <Check className="w-4 h-4 mr-1" />
                                        {isCurrent ? '使用中' : '切換到這個帳號'}
                                    </Button>
                                    <Button size="sm" variant="ghost" className="text-destructive" onClick={() => setConfirming(a)}>
                                        <Trash2 className="w-4 h-4 mr-1" />
                                        解除綁定
                                    </Button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            <Dialog open={!!confirming} onOpenChange={(o) => !o && setConfirming(null)}>
                <DialogContent className="max-w-md">
                    <DialogHeader>
                        <DialogTitle>解除綁定「{confirming?.channelName}」？</DialogTitle>
                    </DialogHeader>
                    <p className="text-sm text-muted-foreground">
                        會刪掉這個後台裡的帳號資料、圖文選單紀錄和排程。LINE 上的圖文選單不會被刪，官方帳號本身也不受影響。
                    </p>
                    <div className="flex justify-end gap-2 pt-2">
                        <Button variant="outline" onClick={() => setConfirming(null)}>
                            取消
                        </Button>
                        <Button
                            variant="destructive"
                            disabled={remove.isPending}
                            onClick={() =>
                                confirming &&
                                remove.mutate(confirming.id, {
                                    onSuccess: () => {
                                        toast.success('已解除綁定');
                                        setConfirming(null);
                                    },
                                })
                            }
                        >
                            解除綁定
                        </Button>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
}
