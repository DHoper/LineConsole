import {
    Bot,
    FileText,
    Home,
    Layout,
    Mail,
    MessageCircle,
    Puzzle,
    Tags,
    Users,
} from "lucide-react";

export const mainMenu = [
    {
        label: "概覽",
        path: "/dashboard",
        icon: Home,
        disabled: false,
    },
    {
        label: "好友管理",
        path: "/members",
        icon: Users,
        disabled: true,
        children: [
            { label: "好友列表", path: "/members/list", disabled: true },
            { label: "篩選與排序", path: "/members/filter", disabled: true },
            { label: "好友類別", path: "/members/types", disabled: true },
            { label: "黑名單", path: "/members/blacklist", disabled: true },
        ],
    },
    {
        label: "標籤管理",
        path: "/tags",
        icon: Tags,
        disabled: true,
    },
    // {
    //     label: "客服中心",
    //     path: "/support-center",
    //     icon: Headphones,
    //     disabled: true,
    //     children: [
    //         { label: "客服設定", path: "/support-center/settings", disabled: true },
    //         { label: "組織說明", path: "/support-center/org", disabled: true },
    //         { label: "成員管理", path: "/support-center/members", disabled: true },
    //         { label: "聊天頁面", path: "/support-center/chat", disabled: true },
    //         { label: "系統訊息", path: "/support-center/system-messages", disabled: true },
    //         { label: "流程設定", path: "/support-center/flows", disabled: true },
    //         { label: "Onboarding", path: "/support-center/onboarding", disabled: true },
    //     ],
    // },
    {
        label: "對話管理",
        path: "/conversations",
        icon: MessageCircle,
        disabled: true,
    },
    {
        label: "發布文章",
        path: "/posts",
        icon: FileText,
        disabled: true,
        children: [
            { label: "文章管理", path: "/posts/list", disabled: true },
            { label: "內容設定", path: "/posts/editor", disabled: true },
        ],
    },
    {
        label: "聊天機器人",
        path: "/chatbot",
        icon: Bot,
        disabled: true,
        children: [
            { label: "機器人管理", path: "/chatbot/list", disabled: true },
            { label: "內容設定", path: "/chatbot/editor", disabled: true },
            { label: "訊息格式", path: "/chatbot/templates", disabled: true },
            { label: "常見問題", path: "/chatbot/faq", disabled: true },
        ],
    },
    {
        label: "圖文選單",
        path: "/rich-menu",
        icon: Layout,
    },
    // {
    //     label: "會員旅程",
    //     path: "/journeys",
    //     icon: Map,
    //     disabled: true,
    // },
    {
        label: "訊息工具",
        path: "/messages",
        icon: Mail,
        disabled: true,
        children: [
            { label: "訊息範本", path: "/messages/templates", disabled: true },
            { label: "追蹤連結", path: "/messages/short-links", disabled: true },
        ],
    },
    {
        label: "進階功能",
        path: "/advanced",
        icon: Puzzle,
        disabled: true,
        children: [
            { label: "分眾圖文選單", path: "/advanced/segmented-menu", disabled: true },
            { label: "優惠碼發送", path: "/advanced/coupon-sender", disabled: true },
            { label: "自動轉客服", path: "/advanced/auto-transfer", disabled: true },
            { label: "即時投票", path: "/advanced/instant-poll", disabled: true },
            { label: "訂閱主題", path: "/advanced/topic-subscription", disabled: true },
            { label: "會員成就", path: "/advanced/member-achievements", disabled: true },
            { label: "優惠券", path: "/advanced/coupons", disabled: true },
            { label: "訊息抽獎", path: "/advanced/lottery-message", disabled: true },
            { label: "抽抽樂", path: "/advanced/lucky-draw", disabled: true },
            { label: "遊樂場", path: "/advanced/amusement-park", disabled: true },
            { label: "好運到", path: "/advanced/lucky-arrival", disabled: true },
            { label: "大螢幕抽獎", path: "/advanced/big-screen-draw", disabled: true },
        ],
    },
];
