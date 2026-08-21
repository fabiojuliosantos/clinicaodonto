<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

import brandLogoDark from '@/assets/images/almeida-branco.png'
import brandLogo from '@/assets/images/almeida-lilas-transparente.png'
import compactLogoDark from '@/assets/images/logo-colapsavel-branco.png'
import compactLogo from '@/assets/images/logo-colapsavel.png'
import { useAuthStore } from '@/modules/auth/auth-store'
import DashboardNavIcon from '@/modules/dashboard/components/DashboardNavIcon.vue'
import ThemeToggle from '@/shared/components/ThemeToggle.vue'

const sidebarStorageKey = 'almeida-sidebar-collapsed'
const router = useRouter()
const authStore = useAuthStore()
const sidebarOpen = ref(false)
const profileOpen = ref(false)
const sidebarCollapsed = ref(readSidebarPreference())

const navGroups = [
  { label: 'Principal', items: [{ icon: 'grid', label: 'Visão geral', active: true }, { icon: 'calendar', label: 'Agenda' }, { icon: 'users', label: 'Pacientes' }] },
  { label: 'Clínica', items: [{ icon: 'tooth', label: 'Atendimentos' }, { icon: 'file', label: 'Prontuários' }, { icon: 'wallet', label: 'Financeiro' }, { icon: 'package', label: 'Estoque' }, { icon: 'chart', label: 'Relatórios' }] },
  { label: 'Administração', items: [{ icon: 'team', label: 'Equipe' }, { icon: 'settings', label: 'Configurações' }] },
]

function readSidebarPreference() {
  if (typeof window === 'undefined') return false
  try {
    return window.localStorage.getItem(sidebarStorageKey) === 'true'
  } catch {
    return false
  }
}

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value
  try {
    window.localStorage.setItem(sidebarStorageKey, String(sidebarCollapsed.value))
  } catch {
    // A preferência continua válida durante a sessão atual.
  }
}

function handleEscape(event: KeyboardEvent) {
  if (event.key !== 'Escape') return
  profileOpen.value = false
  sidebarOpen.value = false
}

async function logout() {
  authStore.clearSession()
  await router.push({ name: 'login' })
}

onMounted(() => document.addEventListener('keydown', handleEscape))
onBeforeUnmount(() => document.removeEventListener('keydown', handleEscape))
</script>

<template>
  <div class="app-shell grid-legibility">
    <aside id="sidebar" class="sidebar" :class="{ open: sidebarOpen, collapsed: sidebarCollapsed }">
      <a class="sidebar__brand" href="#main-content" aria-label="Ir para a visão geral">
        <img :src="brandLogo" alt="Almeida Estética e Sorriso" class="brand-logo brand-logo--expanded brand-logo--light" />
        <img :src="brandLogoDark" alt="Almeida Estética e Sorriso" class="brand-logo brand-logo--expanded brand-logo--dark" />
        <img :src="compactLogo" alt="" aria-hidden="true" class="brand-logo brand-logo--compact brand-logo--light" />
        <img :src="compactLogoDark" alt="" aria-hidden="true" class="brand-logo brand-logo--compact brand-logo--dark" />
      </a>

      <nav class="main-nav" aria-label="Navegação principal">
        <template v-for="group in navGroups" :key="group.label">
          <p>{{ group.label }}</p>
          <button v-for="item in group.items" :key="item.label" type="button" :class="{ active: item.active }" :aria-current="item.active ? 'page' : undefined" :title="sidebarCollapsed ? item.label : undefined" :disabled="!item.active">
            <DashboardNavIcon :name="item.icon" /><span>{{ item.label }}</span>
          </button>
        </template>
      </nav>

      <div class="sidebar__help"><span>?</span><div><strong>Precisa de ajuda?</strong><small>Consulte o guia interno</small></div></div>
      <button class="collapse-button" type="button" aria-controls="sidebar" :aria-expanded="!sidebarCollapsed" :aria-label="sidebarCollapsed ? 'Expandir menu' : 'Recolher menu'" :title="sidebarCollapsed ? 'Expandir menu' : 'Recolher menu'" @click="toggleSidebar">‹</button>
    </aside>

    <main id="main-content" class="workspace">
      <header class="topbar">
        <button class="mobile-menu" type="button" aria-label="Abrir menu" :aria-expanded="sidebarOpen" @click="sidebarOpen = true"><span></span><span></span><span></span></button>
        <label class="search-box"><span aria-hidden="true">⌕</span><input type="search" placeholder="A busca estará disponível em breve" aria-label="Busca global indisponível" disabled /><kbd>⌘ K</kbd></label>
        <div class="topbar__actions">
          <ThemeToggle />
          <button class="icon-button" type="button" aria-label="Notificações indisponíveis" disabled><span aria-hidden="true">♢</span></button>
          <span class="topbar__divider"></span>
          <button class="profile-button" type="button" :aria-expanded="profileOpen" @click="profileOpen = !profileOpen">
            <span class="avatar">AE</span><span class="profile-copy"><strong>Equipe Almeida</strong><small>Acesso interno</small></span><span class="chevron">⌄</span>
          </button>
          <div class="profile-menu" :class="{ show: profileOpen }"><button type="button" disabled>Meu perfil</button><button type="button" disabled>Preferências</button><button type="button" @click="logout">Sair do sistema</button></div>
        </div>
      </header>

      <div class="page-content development-page">
        <section class="development-state" aria-labelledby="development-title">
          <div class="development-illustration" aria-hidden="true">
            <svg viewBox="0 0 96 96" fill="none">
              <path d="M28 67 58 37l8 8-30 30H28v-8Z" />
              <path d="m53 42 8-8a6 6 0 0 1 8 0l2 2a6 6 0 0 1 0 8l-8 8" />
              <path d="M23 27h28M23 39h19M23 51h11" />
              <path d="M20 17h55a7 7 0 0 1 7 7v48a7 7 0 0 1-7 7H20a7 7 0 0 1-7-7V24a7 7 0 0 1 7-7Z" />
            </svg>
            <span>•••</span>
          </div>
          <p class="development-eyebrow">Visão geral</p>
          <h1 id="development-title">O sistema ainda está em desenvolvimento</h1>
          <p>Estamos preparando este ambiente para apoiar a rotina da Clínica Almeida com clareza e segurança.</p>
          <div class="development-status"><span></span> Novos recursos serão disponibilizados gradualmente</div>
        </section>
      </div>
    </main>

    <button v-if="sidebarOpen" class="mobile-overlay" type="button" aria-label="Fechar menu" @click="sidebarOpen = false"></button>
  </div>
</template>
