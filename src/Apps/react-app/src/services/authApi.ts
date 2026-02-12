import api, { API_PREFIX } from '../config/api'

export type UserInfo = {
  displayName: string
  avatarUrl: string
  displayRole: string
  personalSetting?: string
}

export type AuthResult = {
  accessToken: string
  refreshToken?: string | null
  userInfo?: UserInfo
}

export type LoginRequest = {
  username: string
  password: string
}

export type RegisterRequest = {
  email: string
  password: string
  phoneNumber?: string
  fullName?: string
  guestId?: string
}

const reactAuthBase = `${API_PREFIX}/react-auth`

export async function login(request: LoginRequest): Promise<AuthResult> {
  const response = await api.post<AuthResult>(`${reactAuthBase}/login`, request)
  return response.data
}

export async function register(request: RegisterRequest): Promise<AuthResult> {
  const response = await api.post<AuthResult>(`${reactAuthBase}/register`, request)
  return response.data
}

export async function refresh(): Promise<AuthResult> {
  const response = await api.post<AuthResult>(`${reactAuthBase}/refresh`)
  return response.data
}

export async function logout(): Promise<void> {
  await api.post(`${reactAuthBase}/logout`)
}
