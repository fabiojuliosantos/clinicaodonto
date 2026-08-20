import { apiRequest } from '@/shared/api/http-client'

export interface LoginRequest {
  email: string
  senha: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiracao: string
}

export function login(request: LoginRequest) {
  return apiRequest<LoginResponse>('api/Autenticacao/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
