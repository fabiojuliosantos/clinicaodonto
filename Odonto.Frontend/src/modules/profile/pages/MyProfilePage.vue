<script setup lang="ts">
import { computed, ref, watch } from 'vue'

import { useAuthStore } from '@/modules/auth/auth-store'
import { useProfileStore } from '@/modules/profile/profile-store'

const maximumPhotoSize = 2 * 1024 * 1024
const allowedPhotoTypes = new Set(['image/jpeg', 'image/png', 'image/webp'])
const authStore = useAuthStore()
const profileStore = useProfileStore()
const displayName = ref('')
const phone = ref('')
const isSaving = ref(false)
const isUpdatingPhoto = ref(false)
const formError = ref<string | null>(null)
const photoError = ref<string | null>(null)
const successMessage = ref<string | null>(null)

const hasPhoto = computed(() => Boolean(profileStore.profile?.fotoUrl))

watch(
  () => profileStore.profile,
  (profile) => {
    if (!profile) return
    displayName.value = profile.nomeExibicao
    phone.value = profile.telefone ?? ''
  },
  { immediate: true },
)

function token() {
  const value = authStore.session?.token
  if (!value) throw new Error('Sua sessão não está disponível. Entre novamente no sistema.')
  return value
}

async function retryLoad() {
  try {
    await profileStore.load(token())
  } catch {
    // A store mantém a mensagem apresentada no estado de erro.
  }
}

async function saveProfile() {
  formError.value = null
  successMessage.value = null
  const normalizedName = displayName.value.trim()
  if (!normalizedName) {
    formError.value = 'Informe um nome de exibição.'
    return
  }

  isSaving.value = true
  try {
    const profile = await profileStore.update(token(), {
      nomeExibicao: normalizedName,
      telefone: phone.value.trim() || null,
    })
    displayName.value = profile.nomeExibicao
    phone.value = profile.telefone ?? ''
    successMessage.value = 'Dados pessoais atualizados.'
  } catch (error) {
    formError.value = error instanceof Error ? error.message : 'Não foi possível salvar seus dados.'
  } finally {
    isSaving.value = false
  }
}

async function selectPhoto(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  photoError.value = null
  successMessage.value = null
  if (!allowedPhotoTypes.has(file.type)) {
    photoError.value = 'Escolha uma imagem JPEG, PNG ou WebP.'
    return
  }
  if (file.size > maximumPhotoSize) {
    photoError.value = 'A foto deve possuir no máximo 2 MB.'
    return
  }

  isUpdatingPhoto.value = true
  try {
    await profileStore.updatePhoto(token(), file)
    successMessage.value = 'Foto de perfil atualizada.'
  } catch (error) {
    photoError.value = error instanceof Error ? error.message : 'Não foi possível atualizar a foto.'
  } finally {
    isUpdatingPhoto.value = false
  }
}

async function removePhoto() {
  photoError.value = null
  successMessage.value = null
  isUpdatingPhoto.value = true
  try {
    await profileStore.removePhoto(token())
    successMessage.value = 'Foto de perfil removida.'
  } catch (error) {
    photoError.value = error instanceof Error ? error.message : 'Não foi possível remover a foto.'
  } finally {
    isUpdatingPhoto.value = false
  }
}
</script>

<template>
  <div class="page-content profile-page">
    <header class="module-head">
      <div>
        <nav class="breadcrumb" aria-label="Navegação estrutural"><RouterLink :to="{ name: 'dashboard' }">Início</RouterLink><span>/</span><span aria-current="page">Meu perfil</span></nav>
        <h1>Meu perfil</h1>
        <p>Gerencie os dados pessoais e a foto vinculados à sua conta.</p>
      </div>
    </header>

    <section v-if="!profileStore.profile && !profileStore.loadError" class="profile-feedback content-card" aria-live="polite">
      <span class="profile-spinner" aria-hidden="true"></span>
      <h2>Carregando seu perfil</h2>
      <p>Estamos buscando seus dados com segurança.</p>
    </section>

    <section v-else-if="profileStore.loadError && !profileStore.profile" class="profile-feedback content-card" role="alert">
      <span class="profile-feedback__icon" aria-hidden="true">!</span>
      <h2>Não foi possível carregar seu perfil</h2>
      <p>{{ profileStore.loadError }}</p>
      <button class="secondary-button" type="button" :disabled="profileStore.isLoading" @click="retryLoad">Tentar novamente</button>
    </section>

    <div v-else-if="profileStore.profile" class="profile-layout">
      <aside class="content-card profile-summary" aria-label="Resumo do perfil">
        <div class="profile-avatar">
          <img v-if="profileStore.photoObjectUrl" :src="profileStore.photoObjectUrl" alt="Foto de perfil" />
          <span v-else>{{ profileStore.initials }}</span>
        </div>
        <h2>{{ profileStore.profile.nomeExibicao }}</h2>
        <p>{{ profileStore.profile.email }}</p>
        <div class="profile-photo-actions">
          <label class="photo-upload" :class="{ disabled: isUpdatingPhoto }">
            {{ isUpdatingPhoto ? 'Processando foto…' : 'Alterar foto' }}
            <input type="file" accept="image/jpeg,image/png,image/webp" :disabled="isUpdatingPhoto" @change="selectPhoto" />
          </label>
          <button v-if="hasPhoto" class="photo-remove" type="button" :disabled="isUpdatingPhoto" @click="removePhoto">Remover foto</button>
          <small>JPEG, PNG ou WebP de até 2 MB. A imagem será ajustada pelo sistema.</small>
          <p v-if="photoError" class="profile-error" role="alert">{{ photoError }}</p>
        </div>
      </aside>

      <div class="profile-main">
        <section class="content-card profile-section" aria-labelledby="personal-title">
          <div class="profile-section__head">
            <div><h2 id="personal-title">Dados pessoais</h2><p>Informações usadas para identificar você no sistema.</p></div>
          </div>

          <form class="profile-form" @submit.prevent="saveProfile">
            <label>
              <span>Nome completo</span>
              <input :value="profileStore.profile.nomeCompleto" readonly aria-describedby="official-name-help" />
              <small id="official-name-help">Alterações no nome oficial são feitas por um administrador no módulo Equipe.</small>
            </label>
            <label>
              <span>Nome de exibição</span>
              <input v-model="displayName" name="displayName" maxlength="100" autocomplete="name" required />
            </label>
            <label>
              <span>E-mail de acesso</span>
              <input :value="profileStore.profile.email" type="email" readonly aria-describedby="account-email-help" />
              <small id="account-email-help">Vinculado à sua conta de acesso e alterado somente pelo módulo Equipe.</small>
            </label>
            <label>
              <span>Telefone</span>
              <input v-model="phone" name="phone" maxlength="30" inputmode="tel" autocomplete="tel" placeholder="(81) 99999-9999" />
              <small>Opcional. Deixe em branco para remover o telefone atual.</small>
            </label>

            <p v-if="formError" class="profile-form-message profile-error" role="alert">{{ formError }}</p>
            <div class="profile-form-actions">
              <button class="primary-action" type="submit" :disabled="isSaving">
                {{ isSaving ? 'Salvando…' : 'Salvar dados pessoais' }}
              </button>
            </div>
          </form>
        </section>

        <section class="content-card profile-section profile-account-note" aria-labelledby="account-title">
          <div class="profile-section__head">
            <div><h2 id="account-title">Conta e acesso</h2><p>Seus dados administrativos são protegidos contra alterações nesta página.</p></div>
          </div>
          <p>Nome completo e e-mail de acesso devem ser atualizados por uma pessoa autorizada no módulo Equipe.</p>
        </section>
      </div>
    </div>

    <p v-if="successMessage" class="profile-toast" role="status"><span aria-hidden="true">✓</span>{{ successMessage }}</p>
  </div>
</template>
