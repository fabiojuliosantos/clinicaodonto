<script setup lang="ts">
import brandLogoWhite from '@/assets/images/almeida-branco.png'
import brandLogo from '@/assets/images/almeida-lilas-transparente.png'
import ThemeToggle from '@/shared/components/ThemeToggle.vue'
</script>

<template>
  <main class="auth-shell">
    <section class="brand-panel" aria-label="Almeida Estética e Sorriso">
      <div class="brand-panel__overlay"></div>
      <div class="brand-message">
        <span class="eyebrow"><span></span> Sistema interno</span>
        <h1>Gestão clínica com clareza e segurança.</h1>
        <p>Um ambiente de trabalho integrado para toda a equipe Almeida.</p>
      </div>
    </section>

    <section class="form-panel">
      <ThemeToggle class="auth-theme-toggle" />
      <div class="mobile-brand">
        <img :src="brandLogo" alt="Almeida Estética e Sorriso" />
      </div>
      <div class="form-wrap">
        <div class="auth-form-brand" aria-hidden="true">
          <img :src="brandLogo" alt="" class="auth-form-logo auth-form-logo-light" />
          <img :src="brandLogoWhite" alt="" class="auth-form-logo auth-form-logo-dark" />
        </div>
        <slot />
      </div>
      <footer>© 2026 Almeida Estética e Sorriso · Ambiente seguro</footer>
    </section>
  </main>
</template>
