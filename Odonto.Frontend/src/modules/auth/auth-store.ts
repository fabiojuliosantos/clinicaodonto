import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import type { LoginResponse } from '@/modules/auth/auth-api'

const sessionStorageKey = 'almeida-auth-session'

function readStoredSession(): LoginResponse | null {
  try {
    const storedSession = sessionStorage.getItem(sessionStorageKey)
    if (!storedSession) return null

    const value = JSON.parse(storedSession) as Partial<LoginResponse>
    if (
      typeof value.token !== 'string'
      || typeof value.refreshToken !== 'string'
      || typeof value.expiracao !== 'string'
    ) {
      sessionStorage.removeItem(sessionStorageKey)
      return null
    }

    return value as LoginResponse
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const session = ref<LoginResponse | null>(readStoredSession())
  const isAuthenticated = computed(() => {
    if (!session.value) return false
    const expiration = Date.parse(session.value.expiracao)
    return Number.isFinite(expiration) && expiration > Date.now()
  })

  function setSession(value: LoginResponse) {
    session.value = value
    try {
      sessionStorage.setItem(sessionStorageKey, JSON.stringify(value))
    } catch {
      // A sessão continua válida em memória caso o navegador bloqueie o armazenamento.
    }
  }

  function clearSession() {
    session.value = null
    try {
      sessionStorage.removeItem(sessionStorageKey)
    } catch {
      // Não há sessão em memória mesmo quando o armazenamento está indisponível.
    }
  }

  return { session, isAuthenticated, setSession, clearSession }
})
