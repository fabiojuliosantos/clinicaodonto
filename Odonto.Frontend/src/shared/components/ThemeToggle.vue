<script setup lang="ts">
import { useTheme } from '@/shared/composables/use-theme'

const { isDark, toggleTheme } = useTheme()
</script>

<template>
  <button
    class="theme-toggle"
    type="button"
    :aria-label="isDark ? 'Ativar modo claro' : 'Ativar modo escuro'"
    :aria-pressed="isDark"
    title="Alternar tema"
    @click="toggleTheme"
  >
    <span aria-hidden="true">{{ isDark ? '☀' : '☾' }}</span>
  </button>
</template>
