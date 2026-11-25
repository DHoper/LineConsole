import { NavLink } from 'react-router-dom';

import { cn } from '@/libs/ui/utils';
import { mainMenu, plannedMenu } from '@/settings/menu';

/** 左側選單：上面是已完成的功能，下面是規劃中的產品藍圖（看得到、點不進去） */
export default function Sidebar() {
    return (
        <aside className="w-64 h-screen bg-white border-r flex flex-col">
            <div className="p-6 font-bold text-xl text-primary">LineConsole</div>

            <nav className="px-2 space-y-1">
                {mainMenu.map((item) => {
                    const Icon = item.icon;
                    return (
                        <NavLink
                            key={item.label}
                            to={item.path!}
                            className={({ isActive }) =>
                                cn(
                                    'flex items-center gap-3 px-4 py-3 rounded-lg text-gray-600 hover:bg-gray-100 hover:text-primary',
                                    isActive && 'bg-primary/10 text-primary font-semibold',
                                )
                            }
                        >
                            <Icon className="w-5 h-5" />
                            {item.label}
                        </NavLink>
                    );
                })}
            </nav>

            <div className="px-6 pt-6 pb-2 text-xs font-medium text-gray-400">規劃中</div>
            <ul className="px-2 space-y-0.5 overflow-y-auto">
                {plannedMenu.map((item) => {
                    const Icon = item.icon;
                    return (
                        <li
                            key={item.label}
                            title={item.note}
                            aria-disabled="true"
                            className="flex items-center gap-3 px-4 py-2 rounded-lg text-sm text-gray-400 cursor-default select-none"
                        >
                            <Icon className="w-4 h-4" />
                            <span className="flex-1">{item.label}</span>
                            <span className="rounded border border-gray-200 px-1.5 py-0.5 text-[10px] leading-none">規劃中</span>
                        </li>
                    );
                })}
            </ul>
        </aside>
    );
}
