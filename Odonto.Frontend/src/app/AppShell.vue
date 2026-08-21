<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import brandLogoDark from '@/assets/images/almeida-branco.png'
import brandLogo from '@/assets/images/almeida-lilas-transparente.png'
import compactLogoDark from '@/assets/images/logo-colapsavel-branco.png'
import compactLogo from '@/assets/images/logo-colapsavel.png'
import { useAuthStore } from '@/modules/auth/auth-store'
import DashboardNavIcon from '@/modules/dashboard/components/DashboardNavIcon.vue'
import { useProfileStore } from '@/modules/profile/profile-store'
import { ApiRequestError } from '@/shared/api/http-client'
import ThemeToggle from '@/shared/components/ThemeToggle.vue'

const sidebarStorageKey = 'almeida-sidebar-collapsed'
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const profileStore = useProfileStore()
const sidebarOpen = ref(false)
const profileOpen = ref(false)
const profileActions = ref<HTMLElement | null>(null)
const sidebarCollapsed = ref(readSidebarPreference())

const profileName = computed(() => profileStore.profile?.nomeExibicao || 'Equipe Almeida')

const navGroups = [
  { label: 'Principal', items: [{ icon: 'grid', label: 'Visão geral', routeName: 'dashboard' }, { icon: 'calendar', label: 'Agenda' }, { icon: 'users', label: 'Pacientes' }] },
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

function closeTransientNavigation() {
  profileOpen.value = false
  sidebarOpen.value = false
}

function handleEscape(event: KeyboardEvent) {
  if (event.key === 'Escape') closeTransientNavigation()
}

function handleDocumentClick(event: MouseEvent) {
  if (!profileActions.value?.contains(event.target as Node)) profileOpen.value = false
}

async function loadProfile() {
  const token = authStore.session?.token
  if (!token) return

  try {
    await profileStore.load(token)
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 401) await logout()
  }
}

async function logout() {
  profileStore.clear()
  authStore.clearSession()
  await router.push({ name: 'login' })
}

onMounted(() => {
  document.addEventListener('keydown', handleEscape)
  document.addEventListener('click', handleDocumentClick)
  void loadProfile()
})

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleEscape)
  document.removeEventListener('click', handleDocumentClick)
})
</script>

<template>
  <div class="app-shell grid-legibility">
    <aside id="sidebar" class="sidebar" :class="{ open: sidebarOpen, collapsed: sidebarCollapsed }">
      <RouterLink class="sidebar__brand" :to="{ name: 'dashboard' }" aria-label="Ir para a visão geral" @click="closeTransientNavigation">
        <img :src="brandLogo" alt="Almeida Estética e Sorriso" class="brand-logo brand-logo--expanded brand-logo--light" />
        <img :src="brandLogoDark" alt="Almeida Estética e Sorriso" class="brand-logo brand-logo--expanded brand-logo--dark" />
        <img :src="compactLogo" alt="" aria-hidden="true" class="brand-logo brand-logo--compact brand-logo--light" />
        <img :src="compactLogoDark" alt="" aria-hidden="true" class="brand-logo brand-logo--compact brand-logo--dark" />
      </RouterLink>

      <nav class="main-nav" aria-label="Navegação principal">
        <template v-for="group in navGroups" :key="group.label">
          <p>{{ group.label }}</p>
          <template v-for="item in group.items" :key="item.label">
            <RouterLink
              v-if="item.routeName"
              :to="{ name: item.routeName }"
              :class="{ active: route.name === item.routeName }"
              :aria-current="route.name === item.routeName ? 'page' : undefined"
              :title="sidebarCollapsed ? item.label : undefined"
              @click="closeTransientNavigation"
            >
              <DashboardNavIcon :name="item.icon" /><span>{{ item.label }}</span>
            </RouterLink>
            <button v-else type="button" :title="sidebarCollapsed ? item.label : undefined" disabled>
              <DashboardNavIcon :name="item.icon" /><span>{{ item.label }}</span>
            </button>
          </template>
        </template>
      </nav>

      <div class="sidebar__help"><span>?</span><div><strong>Precisa de ajuda?</strong><small>Consulte o guia interno</small></div></div>
      <button class="collapse-button" type="button" aria-controls="sidebar" :aria-expanded="!sidebarCollapsed" :aria-label="sidebarCollapsed ? 'Expandir menu' : 'Recolher menu'" :title="sidebarCollapsed ? 'Expandir menu' : 'Recolher menu'" @click="toggleSidebar">‹</button>
    </aside>

    <main id="main-content" class="workspace">
      <header class="topbar">
        <button class="mobile-menu" type="button" aria-label="Abrir menu" :aria-expanded="sidebarOpen" @click="sidebarOpen = true"><span></span><span></span><span></span></button>
        <label class="search-box"><span aria-hidden="true">⌕</span><input type="search" placeholder="A busca estará disponível em breve" aria-label="Busca global indisponível" disabled /><kbd>⌘ K</kbd></label>
        <div ref="profileActions" class="topbar__actions">
          <ThemeToggle />
          <button class="icon-button" type="button" aria-label="Notificações indisponíveis" disabled><span aria-hidden="true">♢</span></button>
          <span class="topbar__divider"></span>
          <button class="profile-button" type="button" aria-haspopup="menu" :aria-expanded="profileOpen" @click.stop="profileOpen = !profileOpen">
            <span class="avatar">
              <img v-if="profileStore.photoObjectUrl" :src="profileStore.photoObjectUrl" alt="" />
              <span v-else>{{ profileStore.initials }}</span>
            </span>
            <span class="profile-copy"><strong>{{ profileName }}</strong><small>Acesso interno</small></span><span class="chevron">⌄</span>
          </button>
          <div class="profile-menu" :class="{ show: profileOpen }" role="menu">
            <RouterLink :to="{ name: 'my-profile' }" role="menuitem" @click="closeTransientNavigation">Meu perfil</RouterLink>
            <button type="button" role="menuitem" @click="logout">Sair do sistema</button>
          </div>
        </div>
      </header>

      <RouterView />
    </main>

    <button v-if="sidebarOpen" class="mobile-overlay" type="button" aria-label="Fechar menu" @click="sidebarOpen = false"></button>
  </div>
</template>
