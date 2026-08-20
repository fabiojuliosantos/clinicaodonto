import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import { describe, expect, it } from 'vitest'

import LoginPage from '@/modules/auth/pages/LoginPage.vue'

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/login', name: 'login', component: LoginPage },
      { path: '/dashboard', name: 'dashboard', component: { template: '<div>Dashboard</div>' } },
    ],
  })
}

describe('fluxo de autenticação da equipe', () => {
  it('valida as credenciais antes de abrir o dashboard', async () => {
    const router = createTestRouter()
    await router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginPage, { global: { plugins: [createPinia(), router] } })

    await wrapper.get('form').trigger('submit')

    expect(wrapper.findAll('.field.invalid')).toHaveLength(2)
    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('orienta que novos acessos são geridos pela equipe', async () => {
    const router = createTestRouter()
    await router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginPage, { global: { plugins: [createPinia(), router] } })

    expect(wrapper.get('.access-copy').text()).toContain('módulo Equipe')
    expect(wrapper.find('a[href="/cadastro"]').exists()).toBe(false)
  })
})
