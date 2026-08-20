import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import { describe, expect, it } from 'vitest'

import DashboardPage from '@/modules/dashboard/pages/DashboardPage.vue'

describe('tela inicial', () => {
  it('apresenta o estado de desenvolvimento sem dados demonstrativos', async () => {
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/dashboard', name: 'dashboard', component: DashboardPage },
        { path: '/login', name: 'login', component: { template: '<div>Login</div>' } },
      ],
    })
    await router.push('/dashboard')
    await router.isReady()

    const wrapper = mount(DashboardPage, { global: { plugins: [createPinia(), router] } })

    expect(wrapper.get('#development-title').text()).toContain('ainda está em desenvolvimento')
    expect(wrapper.text()).not.toContain('R$')
    expect(wrapper.text()).not.toContain('João da Silva')
    expect(wrapper.text()).not.toContain('atendimentos agendados')
  })
})
