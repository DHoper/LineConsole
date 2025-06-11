import { useEffect } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/features/auth/stores/authStore';

/**
 * 路由守衛：保護內部路由需登入才能訪問。
 * 若未登入或 JWT 已過期，則強制登出並跳轉至登入頁。
 */
export default function RequireAuth() {
  const token = useAuthStore((s) => s.token);
  const isExpired = useAuthStore((s) => s.isExpired);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  useEffect(() => {
    if (!token || isExpired()) {
      logout();
      navigate('/auth/login', { replace: true });
    }
  }, [token, isExpired, logout, navigate]);

  return <Outlet />;
}
