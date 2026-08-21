import { createRouter, createWebHistory, type RouterHistory } from 'vue-router'

import DashboardPage from '@/modules/dashboard/pages/DashboardPage.vue'
import LoginPage from '@/modules/auth/pages/LoginPage.vue'
import PasswordRecoveryPage from '@/modules/auth/pages/PasswordRecoveryPage.vue'
import PasswordResetPage from '@/modules/auth/pages/PasswordResetPage.vue'
import { useAuthStore } from '@/modules/auth/auth-store'

export function createAppRouter(history: RouterHistory = createWebHistory(import.meta.env.BASE_URL)) {
  const router = createRouter({
    history,
    routes: [
      { path: '/', redirect: '/login' },
      { path: '/login', name: 'login', component: LoginPage, meta: { title: 'Entrar', guestOnly: true } },
      { path: '/recuperar-senha', name: 'password-recovery', component: PasswordRecoveryPage, meta: { title: 'Recuperar senha', guestOnly: true } },
      { path: '/redefinir-senha', name: 'password-reset', component: PasswordResetPage, meta: { title: 'Redefinir senha', guestOnly: true } },
      { path: '/dashboard', name: 'dashboard', component: DashboardPage, meta: { title: 'Visão geral', requiresAuth: true } },
      { path: '/:pathMatch(.*)*', redirect: '/login' },
    ],
  })

  router.beforeEach((to) => {
    const authStore = useAuthStore()
    if (authStore.session && !authStore.isAuthenticated) {
      authStore.clearSession()
    }

    if (to.meta.guestOnly && authStore.isAuthenticated) {
      return { name: 'dashboard' }
    }

    if (!to.meta.requiresAuth) return true
    if (authStore.isAuthenticated) return true

    return { name: 'login', query: { redirect: to.fullPath } }
  })

  router.afterEach((to) => {
    document.title = `${String(to.meta.title ?? 'Clínica Odonto')} — Almeida`
  })

  return router
}

const router = createAppRouter()

export default router
