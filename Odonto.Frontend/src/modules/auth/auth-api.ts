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

export interface PasswordRecoveryRequest {
  email: string
}

export interface PasswordRecoveryResponse {
  mensagem: string
}

export interface PasswordResetRequest {
  email: string
  token: string
  novaSenha: string
}

export function login(request: LoginRequest) {
  return apiRequest<LoginResponse>('api/Autenticacao/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function requestPasswordRecovery(request: PasswordRecoveryRequest) {
  return apiRequest<PasswordRecoveryResponse>('api/Autenticacao/redefinir-senha', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function resetPassword(request: PasswordResetRequest) {
  return apiRequest<boolean>('api/Autenticacao/atualizar-senha', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
