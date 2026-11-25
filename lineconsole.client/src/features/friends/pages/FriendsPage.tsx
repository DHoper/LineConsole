import { ChevronLeft, ChevronRight, Search } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { useCurrentAccount } from '@/features/accounts/stores/currentAccount';

import { type FriendQuery, useFriends } from '../api/friendsAPI';
import { formatDateTime, formatNumber } from '../utils/format';

const PAGE_SIZE = 20;
type StatusFilter = 'all' | 'following' | 'blocked';

/** 打字停 300ms 才查詢，不要每個字都打一次 API */
function useDebounced<T>(value: T, ms = 300) {
    const [debounced, setDebounced] = useState(value);
    useEffect(() => {
        const t = setTimeout(() => setDebounced(value), ms);
        return () => clearTimeout(t);
    }, [value, ms]);
    return debounced;
}

export default function FriendsPage() {
    const account = useCurrentAccount();
    const accountId = account?.id ?? '';
    const [keyword, setKeyword] = useState('');
    const [status, setStatus] = useState<StatusFilter>('all');
    const [page, setPage] = useState(1);
    const debouncedKeyword = useDebounced(keyword.trim());

    // 條件變了回到第一頁
    useEffect(() => setPage(1), [debouncedKeyword, status, accountId]);

    const query: FriendQuery = {
        page,
        pageSize: PAGE_SIZE,
        ...(status !== 'all' && { status }),
        ...(debouncedKeyword && { keyword: debouncedKeyword }),
    };
    const { data, isLoading, isError, isFetching } = useFriends(accountId, query);
    const totalPages = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1;

    if (!account) {
        return (
            <div className="p-6 max-w-6xl mx-auto">
                <h1 className="text-2xl font-bold mb-6">好友管理</h1>
                <div className="text-center py-16 border rounded-lg bg-card space-y-4">
                    <p className="text-muted-foreground">還沒有綁定 LINE 官方帳號</p>
                    <Button asChild>
                        <Link to="/accounts">去綁定官方帳號</Link>
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="p-6 space-y-6 max-w-6xl mx-auto">
            <div>
                <h1 className="text-2xl font-bold">好友管理</h1>
                <p className="text-sm text-muted-foreground mt-1">
                    官方帳號：{account.channelName}・好友加入、封鎖、傳訊息時 LINE 會透過 Webhook 通知，名單自動更新
                </p>
            </div>

            {/* 篩選：一列，放在名單上方 */}
            <div className="flex flex-wrap items-center gap-3">
                <div className="relative w-full sm:w-72">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input value={keyword} onChange={(e) => setKeyword(e.target.value)} placeholder="搜尋顯示名稱" className="pl-8" aria-label="搜尋顯示名稱" />
                </div>
                <Select value={status} onValueChange={(v) => setStatus(v as StatusFilter)}>
                    <SelectTrigger className="w-36" aria-label="好友狀態">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">全部</SelectItem>
                        <SelectItem value="following">追蹤中</SelectItem>
                        <SelectItem value="blocked">已封鎖</SelectItem>
                    </SelectContent>
                </Select>
                {data && <span className="text-sm text-muted-foreground">共 {formatNumber(data.total)} 位</span>}
            </div>

            {isLoading ? (
                <p>載入中...</p>
            ) : isError || !data ? (
                <p className="text-red-500">讀取好友名單失敗</p>
            ) : data.items.length === 0 ? (
                <div className="text-center py-16 border rounded-lg bg-card text-muted-foreground">
                    {debouncedKeyword || status !== 'all' ? '沒有符合條件的好友' : '還沒有好友資料：設定好 Webhook 後，好友加入就會出現在這裡'}
                </div>
            ) : (
                <div className={`rounded-lg border bg-card transition-opacity ${isFetching ? 'opacity-70' : ''}`}>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>好友</TableHead>
                                <TableHead>狀態</TableHead>
                                <TableHead>加入時間</TableHead>
                                <TableHead>最後傳訊息</TableHead>
                                <TableHead>封鎖時間</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data.items.map((f) => (
                                <TableRow key={f.id}>
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted text-sm font-medium">
                                                {(f.displayName ?? '?').slice(0, 1)}
                                            </div>
                                            <div className="min-w-0">
                                                <div className="font-medium">{f.displayName ?? '（未取得名稱）'}</div>
                                                <div className="truncate font-mono text-xs text-muted-foreground">{f.lineUserId}</div>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        {f.status === 'Following' ? <Badge variant="secondary">追蹤中</Badge> : <Badge variant="outline">已封鎖</Badge>}
                                    </TableCell>
                                    <TableCell className="tabular-nums">{formatDateTime(f.followedAt)}</TableCell>
                                    <TableCell className="tabular-nums">{formatDateTime(f.lastMessageAt)}</TableCell>
                                    <TableCell className="tabular-nums">{formatDateTime(f.blockedAt)}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                    <div className="flex items-center justify-between border-t px-4 py-3 text-sm">
                        <span className="text-muted-foreground">
                            第 {data.page} / {totalPages} 頁
                        </span>
                        <div className="flex gap-2">
                            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                                <ChevronLeft className="h-4 w-4" />
                                上一頁
                            </Button>
                            <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
                                下一頁
                                <ChevronRight className="h-4 w-4" />
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
