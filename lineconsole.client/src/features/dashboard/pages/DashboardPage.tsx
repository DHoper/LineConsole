import { ArrowRight, Table2 } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';

import { Button } from '@/components/ui/button';
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { useCurrentAccount } from '@/features/accounts/stores/currentAccount';
import { type DailyFriendStat, useDashboard } from '@/features/friends/api/friendsAPI';
import { formatNumber, formatSigned } from '@/features/friends/utils/format';
import { useSystemInfo } from '@/features/system/api/systemAPI';

import { FOLLOW_COLOR, UNFOLLOW_COLOR } from '../components/chartUtils';
import DailyChangeChart from '../components/DailyChangeChart';
import DemoWebhookPanel from '../components/DemoWebhookPanel';
import FriendTrendChart from '../components/FriendTrendChart';

/** 30 天的淨增：今天的人數 − 第一天開始前的人數 */
const netChange = (daily: DailyFriendStat[]) =>
    daily.length ? daily[daily.length - 1].following - (daily[0].following - daily[0].follows + daily[0].unfollows) : 0;

function StatTile({ label, value, hint, hero }: { label: string; value: string; hint?: string; hero?: boolean }) {
    return (
        <Card className="gap-1 py-4">
            <CardContent className="space-y-1 px-4">
                <div className="text-sm text-muted-foreground">{label}</div>
                <div className={hero ? 'text-5xl font-semibold leading-tight' : 'text-2xl font-semibold'}>{value}</div>
                {hint && <div className="text-xs text-muted-foreground">{hint}</div>}
            </CardContent>
        </Card>
    );
}

function LegendItem({ color, label }: { color: string; label: string }) {
    return (
        <span className="inline-flex items-center gap-1.5 text-xs text-muted-foreground">
            <span className="h-2.5 w-2.5 rounded-[2px]" style={{ background: color }} />
            {label}
        </span>
    );
}

export default function DashboardPage() {
    const account = useCurrentAccount();
    const accountId = account?.id ?? '';
    const { data, isLoading, isError, isFetching } = useDashboard(accountId);
    const { data: system } = useSystemInfo();
    const [showTable, setShowTable] = useState(false);

    if (!account) {
        return (
            <div className="p-6 max-w-6xl mx-auto">
                <h1 className="text-2xl font-bold mb-6">概覽</h1>
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
                <h1 className="text-2xl font-bold">概覽</h1>
                <p className="text-sm text-muted-foreground mt-1">官方帳號：{account.channelName}・統計以台北時間計算，資料來自 LINE Webhook</p>
            </div>

            {isLoading ? (
                <p>載入中...</p>
            ) : isError || !data ? (
                <p className="text-red-500">讀取概覽失敗</p>
            ) : (
                // 重抓時保留上一份畫面、只降低不透明度，不閃空白
                <div className={`space-y-6 transition-opacity ${isFetching ? 'opacity-70' : ''}`}>
                    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                        <StatTile hero label="好友人數" value={formatNumber(data.following)} hint={`近 30 天 ${formatSigned(netChange(data.daily))}`} />
                        <StatTile label="今天新增" value={formatNumber(data.followsToday)} hint="加入或解除封鎖" />
                        <StatTile label="今天封鎖" value={formatNumber(data.unfollowsToday)} />
                        <StatTile label="累計封鎖" value={formatNumber(data.blocked)} hint="目前是封鎖狀態的人數" />
                    </div>

                    <Card className="gap-4">
                        <CardHeader>
                            <CardTitle className="text-base">好友人數</CardTitle>
                            <CardDescription>近 30 天，每天結束時的人數</CardDescription>
                            <CardAction>
                                <Button variant="ghost" size="sm" onClick={() => setShowTable((v) => !v)} aria-pressed={showTable}>
                                    <Table2 className="h-4 w-4" />
                                    {showTable ? '隱藏表格' : '以表格檢視'}
                                </Button>
                            </CardAction>
                        </CardHeader>
                        <CardContent>
                            <FriendTrendChart daily={data.daily} />
                        </CardContent>
                    </Card>

                    <Card className="gap-4">
                        <CardHeader>
                            <CardTitle className="text-base">每日新增與封鎖</CardTitle>
                            <CardDescription className="flex flex-wrap gap-4 pt-1">
                                <LegendItem color={FOLLOW_COLOR} label="新增好友（基準線以上）" />
                                <LegendItem color={UNFOLLOW_COLOR} label="封鎖（基準線以下）" />
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            <DailyChangeChart daily={data.daily} />
                        </CardContent>
                    </Card>

                    {showTable && (
                        <Card className="gap-2">
                            <CardHeader>
                                <CardTitle className="text-base">每日明細</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>日期</TableHead>
                                            <TableHead className="text-right">新增</TableHead>
                                            <TableHead className="text-right">封鎖</TableHead>
                                            <TableHead className="text-right">淨增</TableHead>
                                            <TableHead className="text-right">好友人數</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[...data.daily].reverse().map((d) => (
                                            <TableRow key={d.date}>
                                                <TableCell className="tabular-nums">{d.date}</TableCell>
                                                <TableCell className="text-right tabular-nums">{d.follows}</TableCell>
                                                <TableCell className="text-right tabular-nums">{d.unfollows}</TableCell>
                                                <TableCell className="text-right tabular-nums">{formatSigned(d.follows - d.unfollows)}</TableCell>
                                                <TableCell className="text-right tabular-nums">{formatNumber(d.following)}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </CardContent>
                        </Card>
                    )}

                    <div className="grid gap-4 lg:grid-cols-3">
                        <Card className="gap-3">
                            <CardHeader>
                                <CardTitle className="text-base">圖文選單</CardTitle>
                            </CardHeader>
                            <CardContent className="space-y-3">
                                <dl className="grid grid-cols-3 gap-2 text-center">
                                    <div>
                                        <dt className="text-xs text-muted-foreground">選單</dt>
                                        <dd className="text-xl font-semibold">{data.richMenuCount}</dd>
                                    </div>
                                    <div>
                                        <dt className="text-xs text-muted-foreground">待執行排程</dt>
                                        <dd className="text-xl font-semibold">{data.pendingSchedules}</dd>
                                    </div>
                                    <div>
                                        <dt className="text-xs text-muted-foreground">進行中</dt>
                                        <dd className="text-xl font-semibold">{data.activeSchedules}</dd>
                                    </div>
                                </dl>
                                <Button asChild variant="outline" size="sm" className="w-full">
                                    <Link to="/rich-menu">
                                        管理圖文選單
                                        <ArrowRight className="h-4 w-4" />
                                    </Link>
                                </Button>
                            </CardContent>
                        </Card>
                        <div className="lg:col-span-2">
                            {system?.mode === 'Demo' ? (
                                <DemoWebhookPanel accountId={accountId} />
                            ) : (
                                <Card className="gap-3 h-full">
                                    <CardHeader>
                                        <CardTitle className="text-base">Webhook 設定</CardTitle>
                                        <CardDescription>
                                            在 LINE Developers 把 Webhook URL 設成下面的網址並開啟「Use webhook」，好友的加入、封鎖、訊息就會即時進來。
                                        </CardDescription>
                                    </CardHeader>
                                    <CardContent>
                                        <code className="block break-all rounded-md bg-muted px-3 py-2 text-xs">
                                            {`${window.location.origin}/api/webhooks/line/${accountId}`}
                                        </code>
                                    </CardContent>
                                </Card>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
