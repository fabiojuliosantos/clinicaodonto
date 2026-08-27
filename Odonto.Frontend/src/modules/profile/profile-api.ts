import { apiRequest } from '@/shared/api/http-client'

export interface MeuPerfil {
  id: string
  nomeCompleto: string
  nomeExibicao: string
  email: string
  telefone: string | null
  fotoUrl: string | null
}

export interface AtualizarMeuPerfilRequest {
  nomeExibicao: string
  telefone: string | null
}

interface FotoPerfilResponse {
  url: string
}

function authenticatedHeaders(token: string) {
  return { Authorization: `Bearer ${token}` }
}

export function obterMeuPerfil(token: string) {
  return apiRequest<MeuPerfil>('api/me', {
    headers: authenticatedHeaders(token),
  })
}

export function atualizarMeuPerfil(token: string, request: AtualizarMeuPerfilRequest) {
  return apiRequest<MeuPerfil>('api/me', {
    method: 'PATCH',
    headers: authenticatedHeaders(token),
    body: JSON.stringify(request),
  })
}

export function obterFotoPerfil(token: string) {
  return apiRequest<Blob>('api/me/foto', {
    headers: authenticatedHeaders(token),
  }, 'blob')
}

export function atualizarFotoPerfil(token: string, foto: File) {
  const formData = new FormData()
  formData.set('foto', foto)

  return apiRequest<FotoPerfilResponse>('api/me/foto', {
    method: 'PUT',
    headers: authenticatedHeaders(token),
    body: formData,
  })
}

export function removerFotoPerfil(token: string) {
  return apiRequest<void>('api/me/foto', {
    method: 'DELETE',
    headers: authenticatedHeaders(token),
  }, 'void')
}
