import { Outlet } from "react-router-dom";

import Sidebar from "@/layouts/Sidebar";
import TheHeader from "@/layouts/TheHeader";

export default function MainLayout() {
    return (
        <div className="flex w-screen">
            <Sidebar />
            <div className="flex-1 flex flex-col">
                <main className="flex-1 p-6 bg-gray-50 overflow-auto">
                    <TheHeader />
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
