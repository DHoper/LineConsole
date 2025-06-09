import { Loader2 } from "lucide-react";
import { Suspense } from "react";
import { useRoutes } from "react-router-dom";

import { routes } from "./RouteConfig";

export default function AppRouter() {
    return (
        <Suspense fallback={
            <div className="flex h-screen justify-center items-center text-gray-500">
                <Loader2 className="w-6 h-6 animate-spin" />
            </div>
        }>
            {useRoutes(routes)}
        </Suspense>
    );
}
