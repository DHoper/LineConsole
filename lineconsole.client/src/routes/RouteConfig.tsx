import { lazy } from "react";
import { RouteObject } from "react-router-dom";

import ComingSoon from "@/components/ComingSoon";
import MainLayout from "@/layouts/MainLayout";

const RichMenuList = lazy(() => import("@/features/richMenu/pages/RichMenuListPage"));
const LoginPage = lazy(() => import("@/features/auth/pages/LoginPage"));
const RegisterPage = lazy(() => import("@/features/auth/pages/RegisterPage"));

export const routes: RouteObject[] = [
    {
        path: "/auth/login", 
        element: <LoginPage />,
    },
    {
        path: "/auth/register", 
        element: <RegisterPage />,
    },
    {
        path: "/",
        element: <MainLayout />,
        children: [
            { path: "dashboard", element: <ComingSoon /> },

            // 好友管理
            { path: "friends/list", element: <ComingSoon /> },
            { path: "friends/filter", element: <ComingSoon /> },
            { path: "friends/types", element: <ComingSoon /> },
            { path: "friends/blacklist", element: <ComingSoon /> },

            // 標籤管理
            { path: "tags", element: <ComingSoon /> },

            // 對話管理
            { path: "conversations", element: <ComingSoon /> },

            // 發布文章
            { path: "posts/list", element: <ComingSoon /> },
            { path: "posts/editor", element: <ComingSoon /> },

            // 聊天機器人
            { path: "chatbot/list", element: <ComingSoon /> },
            { path: "chatbot/editor", element: <ComingSoon /> },
            { path: "chatbot/templates", element: <ComingSoon /> },
            { path: "chatbot/faq", element: <ComingSoon /> },

            // 圖文選單（已實作）
            { path: "rich-menu", element: <RichMenuList /> },
            // { path: "rich-menu/editor", element: <RichMenuEditor /> },
            // { path: "rich-menu/editor/:id", element: <RichMenuEditor /> }, 


            // 訊息工具
            { path: "messages/templates", element: <ComingSoon /> },
            { path: "messages/short-links", element: <ComingSoon /> },

            // 進階功能
            { path: "advanced/segmented-menu", element: <ComingSoon /> },
            { path: "advanced/coupon-sender", element: <ComingSoon /> },
            { path: "advanced/auto-transfer", element: <ComingSoon /> },
            { path: "advanced/instant-poll", element: <ComingSoon /> },
            { path: "advanced/topic-subscription", element: <ComingSoon /> },
            { path: "advanced/member-achievements", element: <ComingSoon /> },
            { path: "advanced/coupons", element: <ComingSoon /> },
            { path: "advanced/lottery-message", element: <ComingSoon /> },
            { path: "advanced/lucky-draw", element: <ComingSoon /> },
            { path: "advanced/amusement-park", element: <ComingSoon /> },
            { path: "advanced/lucky-arrival", element: <ComingSoon /> },
            { path: "advanced/big-screen-draw", element: <ComingSoon /> },
        ],
    },
];
