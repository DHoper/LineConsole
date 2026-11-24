import { FlaskConical } from 'lucide-react';

import { useSystemInfo } from '../api/systemAPI';

/** 展示模式的提示條：資料是假的、LINE 是模擬的，每次重開程式都會還原 */
export default function DemoBanner() {
    const { data } = useSystemInfo();
    if (data?.mode !== 'Demo') return null;

    return (
        <div className="flex items-center justify-center gap-2 bg-amber-50 px-4 py-1.5 text-xs text-amber-900 border-b border-amber-200">
            <FlaskConical className="h-3.5 w-3.5" />
            展示模式：資料都是假的，LINE 也是模擬的；可以隨意操作，重新啟動程式就會還原。
        </div>
    );
}
