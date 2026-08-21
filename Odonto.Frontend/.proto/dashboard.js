const iconPaths = {
  grid: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',
  calendar: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M8 3v4M16 3v4M3 10h18"/>',
  users: '<path d="M16 21v-2a4 4 0 00-4-4H6a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM22 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"/>',
  tooth: '<path d="M12 5C8 1 3 3 3 8c0 4 2 5 3 10 .5 2 2 3 3 0l1-4c.5-2 3-2 4 0l1 4c1 3 2.5 2 3 0 1-5 3-6 3-10 0-5-5-7-9-3z"/>',
  file: '<path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6M8 13h8M8 17h6"/>',
  wallet: '<path d="M20 7V5a2 2 0 00-2-2H5a3 3 0 000 6h15v11H5a3 3 0 01-3-3V6"/><path d="M16 13h2"/>',
  package: '<path d="M21 16V8a2 2 0 00-1-1.73l-7-4a2 2 0 00-2 0l-7 4A2 2 0 003 8v8a2 2 0 001 1.73l7 4a2 2 0 002 0l7-4A2 2 0 0021 16z"/><path d="M3.3 7 12 12l8.7-5M12 22V12"/>',
  chart: '<path d="M3 3v18h18M7 16l4-5 3 3 5-7"/>',
  team: '<path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM19 8v6M22 11h-6"/>',
  settings: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 00.34 1.88l.06.06-2.83 2.83-.06-.06a1.7 1.7 0 00-1.88-.34 1.7 1.7 0 00-1 1.55V21h-4v-.09A1.7 1.7 0 009 19.37a1.7 1.7 0 00-1.88.34l-.06.06-2.83-2.83.06-.06A1.7 1.7 0 004.63 15a1.7 1.7 0 00-1.55-1H3v-4h.09A1.7 1.7 0 004.63 9a1.7 1.7 0 00-.34-1.88l-.06-.06 2.83-2.83.06.06A1.7 1.7 0 009 4.63h.01A1.7 1.7 0 0010 3.08V3h4v.09A1.7 1.7 0 0015 4.63a1.7 1.7 0 001.88-.34l.06-.06 2.83 2.83-.06.06A1.7 1.7 0 0019.37 9v.01A1.7 1.7 0 0020.92 10H21v4h-.09A1.7 1.7 0 0019.4 15z"/>',
  search: '<circle cx="11" cy="11" r="7"/><path d="M20 20l-4-4"/>',
  bell: '<path d="M18 8a6 6 0 00-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M14 21h-4"/>',
  'user-plus': '<path d="M15 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M8 11a4 4 0 100-8 4 4 0 000 8zM19 8v6M22 11h-6"/>',
  check: '<circle cx="12" cy="12" r="9"/><path d="M8 12l3 3 5-6"/>',
  clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
  filter: '<path d="M4 5h16M7 12h10M10 19h4"/>',
  message: '<path d="M21 15a4 4 0 01-4 4H8l-5 3V7a4 4 0 014-4h10a4 4 0 014 4z"/>',
  return: '<path d="M9 14l-4-4 4-4M5 10h9a5 5 0 010 10h-2"/>'
};

document.querySelectorAll('[data-icon]').forEach(element => {
  const path = iconPaths[element.dataset.icon];
  if (path) element.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${path}</svg>`;
});

const date = new Intl.DateTimeFormat('pt-BR', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' }).format(new Date());
document.querySelector('#current-date').textContent = date;

try {
  const savedProfile = JSON.parse(localStorage.getItem('almeida-user-profile') || '{}');
  if (savedProfile.displayName) {
    const displayName = String(savedProfile.displayName);
    document.querySelector('.profile-copy strong').textContent = displayName;
    document.querySelector('.topbar .avatar').textContent = displayName.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join('').toUpperCase();
    document.querySelector('#welcome-heading').firstChild.textContent = `Bom dia, ${displayName.trim().split(/\s+/)[0]} `;
  }
} catch (_) { /* mantém os dados demonstrativos padrão */ }

const profileButton = document.querySelector('#profile-button');
const profileMenu = document.querySelector('#profile-menu');
profileButton.addEventListener('click', () => {
  const open = profileMenu.classList.toggle('show');
  profileButton.setAttribute('aria-expanded', String(open));
});
document.addEventListener('click', event => {
  if (!event.target.closest('.topbar__actions')) {
    profileMenu.classList.remove('show');
    profileButton.setAttribute('aria-expanded', 'false');
  }
});

const sidebar = document.querySelector('#sidebar');
const collapseButton = document.querySelector('#collapse-sidebar');
const sidebarPreferenceKey = 'almeida-sidebar-collapsed';
sidebar.querySelectorAll('.main-nav a').forEach(link => {
  link.title = link.querySelector(':scope > span:nth-child(2)')?.textContent.trim() || '';
});
function setSidebarCollapsed(collapsed) {
  document.documentElement.classList.toggle('sidebar-state-collapsed', collapsed);
  sidebar.classList.toggle('collapsed', collapsed);
  collapseButton.setAttribute('aria-expanded', String(!collapsed));
  const label = collapsed ? 'Expandir menu' : 'Recolher menu';
  collapseButton.setAttribute('aria-label', label);
  collapseButton.title = label;
  window.syncAlmeidaSidebarLinks?.(collapsed);
}
setSidebarCollapsed(document.documentElement.classList.contains('sidebar-state-collapsed'));
collapseButton.addEventListener('click', () => {
  const collapsed = !sidebar.classList.contains('collapsed');
  setSidebarCollapsed(collapsed);
  try { localStorage.setItem(sidebarPreferenceKey, String(collapsed)); } catch (_) { /* estado segue pelos links */ }
});
const overlay = document.querySelector('#mobile-overlay');
function toggleMobileMenu(force) {
  sidebar.classList.toggle('open', force);
  overlay.classList.toggle('show', force);
}
document.querySelector('#mobile-menu').addEventListener('click', () => toggleMobileMenu(!sidebar.classList.contains('open')));
overlay.addEventListener('click', () => toggleMobileMenu(false));

const modal = document.querySelector('#modal-backdrop');
function showModal(type) {
  const appointment = type === 'appointment';
  document.querySelector('#modal-title').textContent = appointment ? 'Novo agendamento' : 'Novo paciente';
  document.querySelector('#modal-description').textContent = appointment
    ? 'O fluxo completo permitirá selecionar paciente, profissional, procedimento, data e horário.'
    : 'O cadastro completo reunirá os dados pessoais, contatos e informações clínicas do paciente.';
  document.querySelector('#modal-icon').textContent = appointment ? '＋' : '👤';
  modal.classList.add('show');
}
function closeModal() { modal.classList.remove('show'); }
document.querySelector('#new-appointment')?.addEventListener('click', () => showModal('appointment'));
document.querySelector('#new-patient')?.addEventListener('click', () => showModal('patient'));
document.querySelector('#modal-close')?.addEventListener('click', closeModal);
document.querySelector('#modal-confirm')?.addEventListener('click', closeModal);
modal.addEventListener('click', event => { if (event.target === modal) closeModal(); });
document.addEventListener('keydown', event => {
  if (event.key === 'Escape') closeModal();
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
    event.preventDefault();
    document.querySelector('#global-search').focus();
  }
});

document.querySelector('#notification-button').addEventListener('click', () => {
  document.querySelector('#pendencias').scrollIntoView({ behavior: 'smooth', block: 'center' });
});
