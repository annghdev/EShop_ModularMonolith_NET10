import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { GUEST_ID_STORAGE_KEY, clearAccessToken, clearGuestId, setAccessToken, setRefreshTokenHandler } from '../config/api'
import * as authApi from '../services/authApi'
import * as cartApi from '../services/cartApi'
import type { UserInfo } from '../services/authApi'

type AuthContextValue = {
  accessToken: string | null
  userInfo: UserInfo | null
  isAuthenticated: boolean
  isInitializing: boolean
  login: (username: string, password: string) => Promise<void>
  register: (request: authApi.RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  refreshAccessToken: () => Promise<string | null>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function mapUserInfo(userInfo?: UserInfo): UserInfo | null {
  return userInfo ?? null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessTokenValue, setAccessTokenValue] = useState<string | null>(null)
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null)
  const [isInitializing, setIsInitializing] = useState(true)

  const applyToken = useCallback((token: string | null) => {
    setAccessTokenValue(token)

    if (token) {
      setAccessToken(token)
    } else {
      clearAccessToken()
    }
  }, [])

  const clearAuth = useCallback(() => {
    applyToken(null)
    setUserInfo(null)
  }, [applyToken])

  const mergeGuestCartAfterLogin = useCallback(async () => {
    if (typeof window === 'undefined') {
      return
    }

    const guestId = localStorage.getItem(GUEST_ID_STORAGE_KEY)?.trim()
    if (!guestId) {
      return
    }

    try {
      await cartApi.mergeGuestCart(guestId)
      clearGuestId()
      window.dispatchEvent(new CustomEvent('cart:refresh-requested'))
    } catch {
      // Ignore merge failures so login flow can continue.
    }
  }, [])

  const refreshAccessToken = useCallback(async (): Promise<string | null> => {
    try {
      const result = await authApi.refresh()
      applyToken(result.accessToken)
      if (result.userInfo) {
        setUserInfo(mapUserInfo(result.userInfo))
      }

      return result.accessToken
    } catch {
      clearAuth()
      return null
    }
  }, [applyToken, clearAuth])

  useEffect(() => {
    setRefreshTokenHandler(refreshAccessToken)
    return () => setRefreshTokenHandler(null)
  }, [refreshAccessToken])

  useEffect(() => {
    let isMounted = true

    const bootstrapAuth = async () => {
      await refreshAccessToken()
      if (isMounted) {
        setIsInitializing(false)
      }
    }

    void bootstrapAuth()

    return () => {
      isMounted = false
    }
  }, [refreshAccessToken])

  const login = useCallback(async (username: string, password: string) => {
    const result = await authApi.login({ username, password })
    applyToken(result.accessToken)
    await mergeGuestCartAfterLogin()
    setUserInfo(mapUserInfo(result.userInfo))
  }, [applyToken, mergeGuestCartAfterLogin])

  const register = useCallback(async (request: authApi.RegisterRequest) => {
    const result = await authApi.register(request)
    applyToken(result.accessToken)
    await mergeGuestCartAfterLogin()
    setUserInfo(mapUserInfo(result.userInfo))
  }, [applyToken, mergeGuestCartAfterLogin])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      clearAuth()
    }
  }, [clearAuth])

  const value = useMemo<AuthContextValue>(() => ({
    accessToken: accessTokenValue,
    userInfo,
    isAuthenticated: Boolean(accessTokenValue),
    isInitializing,
    login,
    register,
    logout,
    refreshAccessToken,
  }), [accessTokenValue, isInitializing, login, logout, refreshAccessToken, register, userInfo])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
