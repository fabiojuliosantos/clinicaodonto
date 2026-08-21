<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { resetPassword } from '@/modules/auth/auth-api'
import AuthShell from '@/modules/auth/components/AuthShell.vue'
import { ApiRequestError } from '@/shared/api/http-client'

const route = useRoute()
const router = useRouter()
const email = ref(typeof route.query.email === 'string' ? route.query.email : '')
const token = ref('')
const newPassword = ref('')
const passwordConfirmation = ref('')
const showPassword = ref(false)
const submitted = ref(false)
const isSubmitting = ref(false)
const apiError = ref('')

const emailIsValid = computed(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value))
const tokenIsValid = computed(() => /^\d{6}$/.test(token.value))
const passwordIsValid = computed(() => newPassword.value.length >= 6)
const confirmationIsValid = computed(() => passwordConfirmation.value === newPassword.value && passwordConfirmation.value.length > 0)

function normalizeToken() {
  token.value = token.value.replace(/\D/g, '').slice(0, 6)
}

async function submitReset() {
  submitted.value = true
  if (!emailIsValid.value || !tokenIsValid.value || !passwordIsValid.value || !confirmationIsValid.value) return

  apiError.value = ''
  isSubmitting.value = true
  try {
    await resetPassword({ email: email.value, token: token.value, novaSenha: newPassword.value })
    await router.push({ name: 'login', query: { passwordReset: 'success' } })
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 401) {
      apiError.value = 'O código é inválido ou expirou. Solicite um novo código.'
    } else if (error instanceof ApiRequestError && error.status === 400) {
      apiError.value = error.message
    } else if (error instanceof ApiRequestError && error.status === 429) {
      apiError.value = 'Muitas tentativas. Aguarde alguns minutos antes de tentar novamente.'
    } else {
      apiError.value = 'Não foi possível atualizar a senha. Tente novamente.'
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <AuthShell>
    <div class="auth-view">
      <RouterLink class="back-link" :to="{ name: 'password-recovery' }">← <span>Solicitar outro código</span></RouterLink>

      <div class="recovery-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24"><path d="M12 3a9 9 0 1 0 9 9"/><path d="M21 4v6h-6"/><path d="M9 12l2 2 4-5"/></svg>
      </div>
      <div class="form-heading recovery-heading">
        <span class="step-label">Código de verificação</span>
        <h2>Crie uma nova senha</h2>
        <p>Digite o código recebido por e-mail. Ele é válido por dez minutos.</p>
      </div>

      <form novalidate @submit.prevent="submitReset">
        <label class="field" :class="{ invalid: submitted && !emailIsValid }">
          <span>E-mail</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><path d="M4 6h16v12H4zM4 7l8 6 8-6" /></svg>
            <input v-model.trim="email" type="email" autocomplete="email" placeholder="voce@exemplo.com" required />
          </span>
          <small class="error">Informe o e-mail que recebeu o código.</small>
        </label>

        <label class="field" :class="{ invalid: submitted && !tokenIsValid }">
          <span>Código de verificação</span>
          <span class="input-wrap code-input-wrap">
            <input v-model="token" type="text" inputmode="numeric" autocomplete="one-time-code" maxlength="6" placeholder="000000" required @input="normalizeToken" />
          </span>
          <small class="error">O código deve possuir seis dígitos.</small>
        </label>

        <label class="field" :class="{ invalid: submitted && !passwordIsValid }">
          <span>Nova senha</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><rect x="5" y="10" width="14" height="10" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/></svg>
            <input v-model="newPassword" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" placeholder="Digite a nova senha" minlength="6" required />
            <button class="password-toggle" :class="{ visible: showPassword }" type="button" :aria-label="showPassword ? 'Ocultar senha' : 'Mostrar senha'" @click="showPassword = !showPassword"></button>
          </span>
          <small class="error">A senha deve ter ao menos seis caracteres.</small>
        </label>

        <label class="field" :class="{ invalid: submitted && !confirmationIsValid }">
          <span>Confirme a nova senha</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><path d="M5 12l4 4L19 6"/></svg>
            <input v-model="passwordConfirmation" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" placeholder="Digite novamente" required />
          </span>
          <small class="error">As senhas precisam ser iguais.</small>
        </label>

        <p class="password-hint">Use uma combinação de letras maiúsculas e minúsculas, número e caractere especial.</p>
        <p v-if="apiError" class="notice error-notice" role="alert">{{ apiError }}</p>
        <button class="primary-button" type="submit" :disabled="isSubmitting">
          <span>{{ isSubmitting ? 'Atualizando…' : 'Atualizar senha' }}</span><b v-if="!isSubmitting" aria-hidden="true">→</b>
        </button>
      </form>
    </div>
  </AuthShell>
</template>
