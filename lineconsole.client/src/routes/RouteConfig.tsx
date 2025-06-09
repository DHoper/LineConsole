import { lazy } from "react";
import { RouteObject } from "react-router-dom";

import ComingSoon from "@/components/ComingSoon";
import MainLayout from "@/layouts/MainLayout";

const RichMenuList = lazy(() => import("@/features/richMenu/pages/RichMenuListPage"));

export const routes: RouteObject[] = [
    {
        path: "/",
        element: <MainLayout />,
        children: [
            { path: "dashboard", element: <ComingSoon /> },

            // 登入與工作區
            { path: "auth/login", element: <ComingSoon /> },
            { path: "auth/workspace", element: <ComingSoon /> },
            { path: "auth/locale", element: <ComingSoon /> },

            // 會員管理
            { path: "members/list", element: <ComingSoon /> },
            { path: "members/filter", element: <ComingSoon /> },
            { path: "members/types", element: <ComingSoon /> },
            { path: "members/blacklist", element: <ComingSoon /> },

            // 標籤管理
            { path: "tags", element: <ComingSoon /> },

            // 客服中心
            { path: "support-center/settings", element: <ComingSoon /> },
            { path: "support-center/org", element: <ComingSoon /> },
            { path: "support-center/members", element: <ComingSoon /> },
            { path: "support-center/chat", element: <ComingSoon /> },
            { path: "support-center/system-messages", element: <ComingSoon /> },
            { path: "support-center/flows", element: <ComingSoon /> },
            { path: "support-center/onboarding", element: <ComingSoon /> },

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

            // LINE 圖文選單（已實作）
            { path: "rich-menu", element: <RichMenuList /> },
            // { path: "rich-menu/editor", element: <RichMenuEditor /> },
            // { path: "rich-menu/editor/:id", element: <RichMenuEditor /> }, 

            // 會員旅程
            { path: "journeys", element: <ComingSoon /> },

            // 訊息工具
            { path: "messages/templates", element: <ComingSoon /> },
            { path: "messages/short-links", element: <ComingSoon /> },

            // 網頁型工具
            { path: "web-tools/auth-page", element: <ComingSoon /> },
            { path: "web-tools/editor", element: <ComingSoon /> },
            { path: "web-tools/settings", element: <ComingSoon /> },

            // 進階模組
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
