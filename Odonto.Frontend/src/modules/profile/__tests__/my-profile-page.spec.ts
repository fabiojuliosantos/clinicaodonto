import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useAuthStore } from '@/modules/auth/auth-store'
import {
  atualizarFotoPerfil,
  atualizarMeuPerfil,
  obterMeuPerfil,
  type MeuPerfil,
} from '@/modules/profile/profile-api'
import MyProfilePage from '@/modules/profile/pages/MyProfilePage.vue'
import { useProfileStore } from '@/modules/profile/profile-store'

vi.mock('@/modules/profile/profile-api', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/modules/profile/profile-api')>()
  return {
    ...original,
    obterMeuPerfil: vi.fn(),
    obterFotoPerfil: vi.fn(),
    atualizarMeuPerfil: vi.fn(),
    atualizarFotoPerfil: vi.fn(),
    removerFotoPerfil: vi.fn(),
  }
})

const profile: MeuPerfil = {
  id: 'f7904533-c772-4955-811e-4f499ce681af',
  nomeCompleto: 'Julia Guerra de Almeida',
  nomeExibicao: 'Julia Almeida',
  email: 'julia@almeida.com.br',
  telefone: '81999999999',
  fotoUrl: null,
}

describe('Meu perfil', () => {
  beforeEach(() => {
    sessionStorage.clear()
    vi.clearAllMocks()
    setActivePinia(createPinia())
    useAuthStore().setSession({
      token: 'token-valido',
      refreshToken: 'refresh-token',
      expiracao: new Date(Date.now() + 60_000).toISOString(),
    })
  })

  it('carrega o contrato de api/me e calcula as iniciais exibidas no sistema', async () => {
    vi.mocked(obterMeuPerfil).mockResolvedValue(profile)
    const store = useProfileStore()

    await store.load('token-valido')

    expect(obterMeuPerfil).toHaveBeenCalledWith('token-valido')
    expect(store.profile).toEqual(profile)
    expect(store.initials).toBe('JA')
  })

  it('mantém nome completo e e-mail somente leitura e salva apenas os campos permitidos', async () => {
    const profileStore = useProfileStore()
    profileStore.profile = { ...profile }
    vi.mocked(atualizarMeuPerfil).mockResolvedValue({
      ...profile,
      nomeExibicao: 'Ju Almeida',
      telefone: null,
    })
    const wrapper = mount(MyProfilePage, {
      global: {
        stubs: { RouterLink: { template: '<a><slot /></a>' } },
      },
    })

    const readonlyInputs = wrapper.findAll('input[readonly]')
    expect(readonlyInputs).toHaveLength(2)
    expect(readonlyInputs[0]?.element.value).toBe(profile.nomeCompleto)
    expect(readonlyInputs[1]?.element.value).toBe(profile.email)

    await wrapper.get('input[name="displayName"]').setValue('  Ju Almeida  ')
    await wrapper.get('input[name="phone"]').setValue('   ')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(atualizarMeuPerfil).toHaveBeenCalledWith('token-valido', {
      nomeExibicao: 'Ju Almeida',
      telefone: null,
    })
    expect(wrapper.get('[role="status"]').text()).toContain('Dados pessoais atualizados')
  })

  it('rejeita uma foto acima de 2 MB antes de chamar a API', async () => {
    const profileStore = useProfileStore()
    profileStore.profile = { ...profile }
    const wrapper = mount(MyProfilePage, {
      global: {
        stubs: { RouterLink: { template: '<a><slot /></a>' } },
      },
    })
    const file = new File([new Uint8Array((2 * 1024 * 1024) + 1)], 'foto.png', {
      type: 'image/png',
    })

    const input = wrapper.get('input[type="file"]')
    Object.defineProperty(input.element, 'files', {
      configurable: true,
      value: [file],
    })
    await input.trigger('change')

    expect(wrapper.get('[role="alert"]').text()).toContain('no máximo 2 MB')
    expect(atualizarFotoPerfil).not.toHaveBeenCalled()
  })
})
