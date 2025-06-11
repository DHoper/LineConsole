// src/features/auth/hooks/useLoginMutation.ts
import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { authAPI } from '@/features/auth/api/authAPI'
import { LoginInput } from '@/features/auth/types'
import { useAuth } from '@/features/auth/stores/useAuth'

/**
 * 封裝登入邏輯，呼叫登入 API 並更新狀態
 */
export function useLoginMutation() {
    const { login } = useAuth()

    return useMutation({
        mutationFn: (input: LoginInput) => authAPI.login(input),
        onSuccess: (response) => {
            if (!response.data) {
                toast.error('登入回應異常，缺少使用者資訊')
                return
            }

            login(response.data)
            toast.success('登入成功')
        },
        onError: (error) => {
            console.error('[useLoginMutation] 登入失敗：', error)
            toast.error('登入失敗，請確認帳號密碼')
        },
    })
}
