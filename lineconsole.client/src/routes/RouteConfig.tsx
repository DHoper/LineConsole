import { lazy } from 'react';
import { Navigate, RouteObject } from 'react-router-dom';

import MainLayout from '@/layouts/MainLayout';

import RequireAuth from './RequireAuth';

const AccountsPage = lazy(() => import('@/features/accounts/pages/AccountsPage'));
const DashboardPage = lazy(() => import('@/features/dashboard/pages/DashboardPage'));
const FriendsPage = lazy(() => import('@/features/friends/pages/FriendsPage'));
const RichMenuList = lazy(() => import('@/features/richMenu/pages/RichMenuListPage'));
const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage'));
const RegisterPage = lazy(() => import('@/features/auth/pages/RegisterPage'));

/**
 * 應用程式所有路由（React Router）。
 * 只有已完成的功能有路由；規劃中的功能只出現在左側選單，不做空頁面。
 */
export const routes: RouteObject[] = [
    // 公開路由：登入與註冊
    { path: '/auth/login', element: <LoginPage /> },
    { path: '/auth/register', element: <RegisterPage /> },

    // 需要登入
    {
        path: '/',
        element: <RequireAuth />,
        children: [
            {
                element: <MainLayout />,
                children: [
                    { index: true, element: <Navigate to="/dashboard" replace /> },
                    { path: 'dashboard', element: <DashboardPage /> },
                    { path: 'rich-menu', element: <RichMenuList /> },
                    { path: 'friends', element: <FriendsPage /> },
                    { path: 'accounts', element: <AccountsPage /> },
                    { path: '*', element: <Navigate to="/dashboard" replace /> },
                ],
            },
        ],
    },
];
