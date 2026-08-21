<script setup lang="ts">
import { computed, ref } from 'vue'

import { requestPasswordRecovery } from '@/modules/auth/auth-api'
import AuthShell from '@/modules/auth/components/AuthShell.vue'

const email = ref('')
const submitted = ref(false)
const isSubmitting = ref(false)
const apiError = ref('')
const requestAccepted = ref(false)

const emailIsValid = computed(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value))

async function submitRecovery() {
  submitted.value = true
  if (!emailIsValid.value) return

  apiError.value = ''
  isSubmitting.value = true
  try {
    await requestPasswordRecovery({ email: email.value })
    requestAccepted.value = true
  } catch {
    apiError.value = 'Não foi possível solicitar o código. Verifique sua conexão e tente novamente.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <AuthShell>
    <div class="auth-view">
      <RouterLink class="back-link" :to="{ name: 'login' }">← <span>Voltar para o login</span></RouterLink>

      <div class="recovery-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24"><path d="M5 11V8a7 7 0 0 1 14 0v3M4 11h16v10H4z"/><path d="M12 15v2"/></svg>
      </div>
      <div class="form-heading recovery-heading">
        <span class="step-label">Recuperação de acesso</span>
        <h2>Esqueceu sua senha?</h2>
        <p>Informe seu e-mail profissional para receber um código de verificação.</p>
      </div>

      <div v-if="requestAccepted" class="recovery-confirmation" role="status" aria-live="polite">
        <span class="confirmation-mark" aria-hidden="true">✓</span>
        <h3>Solicitação recebida</h3>
        <p>Se <strong>{{ email }}</strong> estiver cadastrado, enviaremos um código válido por dez minutos.</p>
        <p>Confira também a caixa de spam antes de solicitar novamente.</p>
        <RouterLink class="primary-button" :to="{ name: 'password-reset', query: { email } }">
          <span>Informar código</span><b aria-hidden="true">→</b>
        </RouterLink>
        <button class="text-button confirmation-retry" type="button" @click="requestAccepted = false">Usar outro e-mail</button>
      </div>

      <form v-else novalidate @submit.prevent="submitRecovery">
        <label class="field" :class="{ invalid: submitted && !emailIsValid }">
          <span>E-mail</span>
          <span class="input-wrap">
            <svg aria-hidden="true" viewBox="0 0 24 24"><path d="M4 6h16v12H4zM4 7l8 6 8-6" /></svg>
            <input v-model.trim="email" type="email" autocomplete="email" placeholder="voce@exemplo.com" required />
          </span>
          <small class="error">Informe um e-mail válido.</small>
        </label>

        <p class="privacy-note">Por segurança, a confirmação será a mesma mesmo que o e-mail não esteja cadastrado.</p>
        <p v-if="apiError" class="notice error-notice" role="alert">{{ apiError }}</p>
        <button class="primary-button" type="submit" :disabled="isSubmitting">
          <span>{{ isSubmitting ? 'Enviando…' : 'Enviar código' }}</span><b v-if="!isSubmitting" aria-hidden="true">→</b>
        </button>
      </form>
    </div>
  </AuthShell>
</template>
