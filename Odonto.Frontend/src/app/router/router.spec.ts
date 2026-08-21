import { createPinia, setActivePinia } from 'pinia'
import { createMemoryHistory } from 'vue-router'
import { beforeEach, describe, expect, it } from 'vitest'

import { createAppRouter } from '@/app/router'
import { useAuthStore } from '@/modules/auth/auth-store'

describe('proteção das rotas privadas', () => {
  beforeEach(() => {
    sessionStorage.clear()
    setActivePinia(createPinia())
  })

  it('redireciona um visitante sem sessão para o login', async () => {
    const router = createAppRouter(createMemoryHistory())

    await router.push('/dashboard')

    expect(router.currentRoute.value.name).toBe('login')
    expect(router.currentRoute.value.query.redirect).toBe('/dashboard')
  })

  it('permite acessar o dashboard com uma sessão válida', async () => {
    const authStore = useAuthStore()
    authStore.setSession({
      token: 'token-valido',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() + 60_000).toISOString(),
    })
    const router = createAppRouter(createMemoryHistory())

    await router.push('/dashboard')

    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  it('limpa a sessão expirada e redireciona para o login', async () => {
    const authStore = useAuthStore()
    authStore.setSession({
      token: 'token-expirado',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() - 60_000).toISOString(),
    })
    const router = createAppRouter(createMemoryHistory())

    await router.push('/dashboard')

    expect(router.currentRoute.value.name).toBe('login')
    expect(authStore.session).toBeNull()
  })

  it.each(['/login', '/recuperar-senha', '/redefinir-senha'])(
    'redireciona uma sessão válida de %s para o dashboard',
    async (path) => {
      const authStore = useAuthStore()
      authStore.setSession({
        token: 'token-valido',
        refreshToken: 'refresh-token',
        expiracao: new Date(Date.now() + 60_000).toISOString(),
      })
      const router = createAppRouter(createMemoryHistory())

      await router.push(path)

      expect(router.currentRoute.value.name).toBe('dashboard')
    },
  )

  it('limpa uma sessão expirada e permite acessar o login', async () => {
    const authStore = useAuthStore()
    authStore.setSession({
      token: 'token-expirado',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() - 60_000).toISOString(),
    })
    const router = createAppRouter(createMemoryHistory())

    await router.push('/login')

    expect(router.currentRoute.value.name).toBe('login')
    expect(authStore.session).toBeNull()
  })

  it('restaura a sessão da aba após a aplicação ser recarregada', async () => {
    const authStore = useAuthStore()
    authStore.setSession({
      token: 'token-valido',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() + 60_000).toISOString(),
    })

    setActivePinia(createPinia())
    const restoredAuthStore = useAuthStore()
    const router = createAppRouter(createMemoryHistory())
    await router.push('/login')

    expect(restoredAuthStore.isAuthenticated).toBe(true)
    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  it('remove a sessão persistida ao deslogar', () => {
    const authStore = useAuthStore()
    authStore.setSession({
      token: 'token-valido',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() + 60_000).toISOString(),
    })

    authStore.clearSession()
    setActivePinia(createPinia())

    expect(useAuthStore().session).toBeNull()
  })
})
