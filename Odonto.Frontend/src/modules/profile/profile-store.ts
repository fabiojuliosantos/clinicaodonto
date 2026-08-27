import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import {
  atualizarFotoPerfil,
  atualizarMeuPerfil,
  obterFotoPerfil,
  obterMeuPerfil,
  removerFotoPerfil,
  type AtualizarMeuPerfilRequest,
  type MeuPerfil,
} from '@/modules/profile/profile-api'

function errorMessage(error: unknown) {
  return error instanceof Error
    ? error.message
    : 'Não foi possível concluir a solicitação.'
}

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<MeuPerfil | null>(null)
  const photoObjectUrl = ref<string | null>(null)
  const isLoading = ref(false)
  const loadError = ref<string | null>(null)
  let loadPromise: Promise<void> | null = null

  const initials = computed(() => {
    const name = profile.value?.nomeExibicao.trim()
    if (!name) return 'AE'

    const words = name.split(/\s+/).filter(Boolean)
    const first = words[0]?.charAt(0) ?? ''
    const last = words.length > 1 ? words.at(-1)?.charAt(0) ?? '' : ''
    return `${first}${last}`.toLocaleUpperCase('pt-BR') || 'AE'
  })

  function replacePhotoObjectUrl(blob: Blob | null) {
    if (photoObjectUrl.value) URL.revokeObjectURL(photoObjectUrl.value)
    photoObjectUrl.value = blob ? URL.createObjectURL(blob) : null
  }

  async function loadPhoto(token: string) {
    if (!profile.value?.fotoUrl) {
      replacePhotoObjectUrl(null)
      return
    }

    try {
      replacePhotoObjectUrl(await obterFotoPerfil(token))
    } catch {
      replacePhotoObjectUrl(null)
    }
  }

  function load(token: string) {
    if (loadPromise) return loadPromise

    loadPromise = (async () => {
      isLoading.value = true
      loadError.value = null
      try {
        profile.value = await obterMeuPerfil(token)
        await loadPhoto(token)
      } catch (error) {
        loadError.value = errorMessage(error)
        throw error
      } finally {
        isLoading.value = false
        loadPromise = null
      }
    })()

    return loadPromise
  }

  async function update(token: string, request: AtualizarMeuPerfilRequest) {
    profile.value = await atualizarMeuPerfil(token, request)
    return profile.value
  }

  async function updatePhoto(token: string, file: File) {
    const result = await atualizarFotoPerfil(token, file)
    if (profile.value) profile.value.fotoUrl = result.url
    await loadPhoto(token)
  }

  async function removePhoto(token: string) {
    await removerFotoPerfil(token)
    if (profile.value) profile.value.fotoUrl = null
    replacePhotoObjectUrl(null)
  }

  function clear() {
    profile.value = null
    loadError.value = null
    replacePhotoObjectUrl(null)
  }

  return {
    profile,
    photoObjectUrl,
    isLoading,
    loadError,
    initials,
    load,
    update,
    updatePhoto,
    removePhoto,
    clear,
  }
})
