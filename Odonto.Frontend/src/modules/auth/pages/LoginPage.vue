<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

import AuthShell from '@/modules/auth/components/AuthShell.vue'
import { login } from '@/modules/auth/auth-api'
import { useAuthStore } from '@/modules/auth/auth-store'
import { ApiRequestError } from '@/shared/api/http-client'

const router = useRouter()
const authStore = useAuthStore()
const email = ref('')
const password = ref('')
const remember = ref(false)
const showPassword = ref(false)
const submitted = ref(false)
const recoveryMessage = ref('')
const apiError = ref('')
const isSubmitting = ref(false)

const emailIsValid = computed(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value))
const passwordIsValid = computed(() => password.value.length >= 6)

async function submitLogin() {
  submitted.value = true
  if (!emailIsValid.value || !passwordIsValid.value) return
  apiError.value = ''
  isSubmitting.value = true
  try {
    const session = await login({ email: email.value, senha: password.value })
    authStore.setSession(session)
    await router.push({ name: 'dashboard' })
  } catch (error) {
    apiError.value = error instanceof ApiRequestError && error.status === 401
      ? 'E-mail ou senha inválidos.'
      : 'Não foi possível entrar. Verifique se a API está disponível e tente novamente.'
  } finally {
    isSubmitting.value = false
  }
}

function showRecoveryNotice() {
  recoveryMessage.value = 'A recuperação de senha será disponibilizada após a integração com a API.'
}
</script>

<template>
  <AuthShell>
    <div class="auth-view">
      <div class="form-heading">
        <h2>Acesse sua conta</h2>
        <p>Ambiente exclusivo para a equipe.</p>
      </div>

      <form novalidate @submit.prevent="submitLogin">
        <label class="field" :class="{ invalid: submitted && !emailIsValid }">
          <span>E-mail</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><path d="M4 6h16v12H4zM4 7l8 6 8-6" /></svg>
            <input v-model.trim="email" type="email" autocomplete="email" placeholder="voce@exemplo.com" required />
          </span>
          <small class="error">Informe um e-mail válido.</small>
        </label>

        <label class="field" :class="{ invalid: submitted && !passwordIsValid }">
          <span>Senha</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><rect x="5" y="10" width="14" height="10" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/></svg>
            <input v-model="password" :type="showPassword ? 'text' : 'password'" autocomplete="current-password" placeholder="Digite sua senha" minlength="6" required />
            <button class="password-toggle" :class="{ visible: showPassword }" type="button" :aria-label="showPassword ? 'Ocultar senha' : 'Mostrar senha'" @click="showPassword = !showPassword"></button>
          </span>
          <small class="error">A senha deve ter ao menos 6 caracteres.</small>
        </label>

        <div class="form-options">
          <label class="check"><input v-model="remember" type="checkbox" /><span></span>Lembrar de mim</label>
          <button type="button" class="text-button" @click="showRecoveryNotice">Esqueci minha senha</button>
        </div>
        <p v-if="recoveryMessage" class="inline-message" role="status">{{ recoveryMessage }}</p>
        <p v-if="apiError" class="notice error-notice" role="alert">{{ apiError }}</p>
        <button class="primary-button" type="submit" :disabled="isSubmitting"><span>{{ isSubmitting ? 'Entrando…' : 'Entrar no sistema' }}</span><b v-if="!isSubmitting" aria-hidden="true">→</b></button>
      </form>

      <p class="access-copy">Precisa de acesso? Procure um responsável pelo módulo Equipe.</p>
    </div>
  </AuthShell>
</template>
