import { Bell, Search } from "lucide-react";

export default function TheHeader() {
    return (
        <header className="flex items-center justify-between bg-white dark:bg-background h-16 px-6 shadow-sm border-b border-border">
            <div className="flex items-center gap-4">
                <Search className="w-5 h-5 text-gray-500 dark:text-gray-300" />
                <input
                    placeholder="Search or type command..."
                    className="border-none outline-none bg-transparent text-sm text-gray-800 dark:text-gray-100"
                />
            </div>
            <div className="flex items-center gap-4">
                <Bell className="w-5 h-5 text-gray-500 dark:text-gray-300 cursor-pointer" />
                <img
                    src="https://i.pravatar.cc/32"
                    className="w-8 h-8 rounded-full object-cover"
                />
            </div>
        </header>
    );
}
