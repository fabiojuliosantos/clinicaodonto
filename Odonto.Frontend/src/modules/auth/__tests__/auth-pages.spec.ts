import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import { describe, expect, it, vi } from 'vitest'

import LoginPage from '@/modules/auth/pages/LoginPage.vue'
import PasswordRecoveryPage from '@/modules/auth/pages/PasswordRecoveryPage.vue'
import PasswordResetPage from '@/modules/auth/pages/PasswordResetPage.vue'

const authApi = vi.hoisted(() => ({
  requestPasswordRecovery: vi.fn(),
  resetPassword: vi.fn(),
}))

vi.mock('@/modules/auth/auth-api', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/modules/auth/auth-api')>()
  return { ...original, ...authApi }
})

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/login', name: 'login', component: LoginPage },
      { path: '/recuperar-senha', name: 'password-recovery', component: PasswordRecoveryPage },
      { path: '/redefinir-senha', name: 'password-reset', component: PasswordResetPage },
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

  it('abre a solicitação de recuperação pelo login', async () => {
    const router = createTestRouter()
    await router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginPage, { global: { plugins: [createPinia(), router] } })

    await wrapper.get('a.text-button').trigger('click')
    await vi.waitFor(() => expect(router.currentRoute.value.name).toBe('password-recovery'))

    expect(router.currentRoute.value.name).toBe('password-recovery')
  })

  it('confirma a solicitação sem revelar se o e-mail existe e permite avançar', async () => {
    authApi.requestPasswordRecovery.mockResolvedValueOnce({ mensagem: 'Solicitação recebida.' })
    const router = createTestRouter()
    await router.push('/recuperar-senha')
    await router.isReady()
    const wrapper = mount(PasswordRecoveryPage, { global: { plugins: [createPinia(), router] } })

    await wrapper.get('input[type="email"]').setValue('equipe@almeida.com')
    await wrapper.get('form').trigger('submit')
    await vi.waitFor(() => expect(wrapper.find('.recovery-confirmation').exists()).toBe(true))

    expect(authApi.requestPasswordRecovery).toHaveBeenCalledWith({ email: 'equipe@almeida.com' })
    expect(wrapper.get('.recovery-confirmation').text()).toContain('Se equipe@almeida.com estiver cadastrado')
    expect(router.currentRoute.value.name).toBe('password-recovery')

    await wrapper.get('.recovery-confirmation a.primary-button').trigger('click')
    await vi.waitFor(() => expect(router.currentRoute.value.name).toBe('password-reset'))
    expect(router.currentRoute.value.query.email).toBe('equipe@almeida.com')
  })

  it('valida o código e a confirmação antes de atualizar a senha', async () => {
    const router = createTestRouter()
    await router.push('/redefinir-senha?email=equipe@almeida.com')
    await router.isReady()
    const wrapper = mount(PasswordResetPage, { global: { plugins: [createPinia(), router] } })

    await wrapper.get('input[autocomplete="one-time-code"]').setValue('12a')
    await wrapper.get('form').trigger('submit')

    expect((wrapper.get('input[autocomplete="one-time-code"]').element as HTMLInputElement).value).toBe('12')
    expect(wrapper.findAll('.field.invalid').length).toBeGreaterThan(0)
    expect(authApi.resetPassword).not.toHaveBeenCalled()
  })

  it('atualiza a senha e retorna ao login com confirmação', async () => {
    authApi.resetPassword.mockResolvedValueOnce(true)
    const router = createTestRouter()
    await router.push('/redefinir-senha?email=equipe@almeida.com')
    await router.isReady()
    const wrapper = mount(PasswordResetPage, { global: { plugins: [createPinia(), router] } })

    await wrapper.get('input[autocomplete="one-time-code"]').setValue('123456')
    const passwordInputs = wrapper.findAll('input[autocomplete="new-password"]')
    await passwordInputs[0]!.setValue('Nova@123')
    await passwordInputs[1]!.setValue('Nova@123')
    await wrapper.get('form').trigger('submit')
    await vi.waitFor(() => expect(router.currentRoute.value.name).toBe('login'))

    expect(authApi.resetPassword).toHaveBeenCalledWith({
      email: 'equipe@almeida.com',
      token: '123456',
      novaSenha: 'Nova@123',
    })
    expect(router.currentRoute.value.query.passwordReset).toBe('success')
  })
})
