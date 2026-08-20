import { createRouter, createWebHistory } from 'vue-router'

import DashboardPage from '@/modules/dashboard/pages/DashboardPage.vue'
import LoginPage from '@/modules/auth/pages/LoginPage.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/login' },
    { path: '/login', name: 'login', component: LoginPage, meta: { title: 'Entrar' } },
    { path: '/dashboard', name: 'dashboard', component: DashboardPage, meta: { title: 'Visão geral' } },
    { path: '/:pathMatch(.*)*', redirect: '/login' },
  ],
})

router.afterEach((to) => {
  document.title = `${String(to.meta.title ?? 'Clínica Odonto')} — Almeida`
})

export default router
