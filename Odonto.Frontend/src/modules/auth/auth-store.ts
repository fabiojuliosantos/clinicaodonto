import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import type { LoginResponse } from '@/modules/auth/auth-api'

export const useAuthStore = defineStore('auth', () => {
  const session = ref<LoginResponse | null>(null)
  const isAuthenticated = computed(() => session.value !== null)

  function setSession(value: LoginResponse) {
    session.value = value
  }

  function clearSession() {
    session.value = null
  }

  return { session, isAuthenticated, setSession, clearSession }
})
