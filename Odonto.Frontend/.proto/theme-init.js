(() => {
  const fromUrl = new URLSearchParams(location.search).get('theme');
  const sidebarFromUrl = new URLSearchParams(location.search).get('sidebar');
  let saved = null;

  try { saved = localStorage.getItem('almeida-theme'); } catch (_) { /* file:// pode isolar o armazenamento */ }

  const theme = fromUrl === 'dark' || fromUrl === 'light'
    ? fromUrl
    : saved || (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');

  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;

  let storedSidebar = null;
  try { storedSidebar = localStorage.getItem('almeida-sidebar-collapsed'); } catch (_) { /* file:// pode isolar o armazenamento */ }
  const collapsed = sidebarFromUrl === 'collapsed' || (sidebarFromUrl !== 'expanded' && storedSidebar === 'true');
  if (collapsed) document.documentElement.classList.add('sidebar-state-collapsed');
})();
