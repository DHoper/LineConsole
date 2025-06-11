import { useStore } from "zustand";
import { useAuthStore } from "@/features/auth/stores/authStore";

export const useAuth = () => {
    const token = useStore(useAuthStore, (state) => state.token);
    const user = useStore(useAuthStore, (state) => state.user);
    const login = useStore(useAuthStore, (state) => state.login);
    const logout = useStore(useAuthStore, (state) => state.logout);
    const isExpired = useStore(useAuthStore, (state) => state.isExpired);

    const isAuthenticated = !!token && !!user && !isExpired();

    return {
        token,
        user,
        login,
        logout,
        isExpired,
        isAuthenticated,
    };
};
