import { ChevronDown } from "lucide-react";
import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";

import { cn } from "@/libs/ui/utils";
import { mainMenu } from "@/navigation/menu";

export default function Sidebar() {
    const location = useLocation();
    const [open, setOpen] = useState<string | null>(null);

    return (
        <aside className="w-64 h-screen bg-white border-r">
            <div className="p-6 font-bold text-xl text-primary">TailAdmin</div>
            <nav className="px-2 space-y-1">
                {mainMenu.map((item) => {
                    const Icon = item.icon;
                    const isActive = location.pathname.startsWith(item.path || "");

                    return (
                        <div key={item.label}>
                            {item.children ? (
                                <div
                                    className={cn(
                                        "flex items-center justify-between px-4 py-3 rounded-lg cursor-pointer hover:bg-gray-100 text-gray-600",
                                        isActive && "bg-primary/10 text-primary font-semibold"
                                    )}
                                    onClick={() =>
                                        setOpen(open === item.label ? null : item.label)
                                    }
                                >
                                    <div className="flex items-center gap-3">
                                        <Icon className="w-5 h-5" />
                                        {item.label}
                                    </div>
                                    <ChevronDown
                                        className={cn(
                                            "w-4 h-4 transition-transform",
                                            open === item.label && "rotate-180"
                                        )}
                                    />
                                </div>
                            ) : (
                                <NavLink
                                    to={item.path || "#"}
                                    className={({ isActive }) =>
                                        cn(
                                            "flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-100 text-gray-600 hover:text-primary",
                                            isActive && "bg-primary/10 text-primary font-semibold"
                                        )
                                    }
                                >
                                    <Icon className="w-5 h-5" />
                                    {item.label}
                                </NavLink>
                            )}

                            {item.children && open === item.label && (
                                <div className="ml-8 mt-2 space-y-1 text-[15px]">
                                    {item.children.map((child) => (
                                        <NavLink
                                            to={child.path}
                                            key={child.label}
                                            className={({ isActive }) =>
                                                cn(
                                                    "block px-3 py-1 rounded hover:bg-gray-100 text-gray-600 hover:text-primary",
                                                    isActive && "text-primary font-medium"
                                                )
                                            }
                                        >
                                            {child.label}
                                        </NavLink>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                })}
            </nav>
        </aside>
    );
}
