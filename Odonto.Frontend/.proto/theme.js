(() => {
  const root = document.documentElement;
  const themeFromUrl = new URLSearchParams(location.search).get('theme');
  let saved = null;
  try { saved = localStorage.getItem('almeida-theme'); } catch (_) { /* file:// pode isolar o armazenamento */ }
  const preferred = matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';

  function syncInternalLinks(theme, collapsed = root.classList.contains('sidebar-state-collapsed')) {
    document.querySelectorAll('a[href]').forEach(link => {
      const rawHref = link.getAttribute('href');
      if (!rawHref || rawHref.startsWith('#') || /^(https?:|mailto:|tel:)/.test(rawHref)) return;
      const target = new URL(rawHref, location.href);
      if (!target.pathname.endsWith('.html')) return;
      target.searchParams.set('theme', theme);
      target.searchParams.set('sidebar', collapsed ? 'collapsed' : 'expanded');
      link.setAttribute('href', `${target.pathname.split('/').pop()}${target.search}${target.hash}`);
    });
  }

  window.syncAlmeidaSidebarLinks = collapsed => syncInternalLinks(root.dataset.theme || preferred, collapsed);

  window.withAlmeidaTheme = href => {
    const target = new URL(href, location.href);
    target.searchParams.set('theme', root.dataset.theme || preferred);
    target.searchParams.set('sidebar', root.classList.contains('sidebar-state-collapsed') ? 'collapsed' : 'expanded');
    return `${target.pathname.split('/').pop()}${target.search}${target.hash}`;
  };

  function apply(theme) {
    root.dataset.theme = theme;
    root.style.colorScheme = theme;
    document.querySelectorAll('.theme-toggle').forEach(button => {
      const dark = theme === 'dark';
      button.textContent = dark ? '☀' : '☾';
      button.setAttribute('aria-label', dark ? 'Ativar modo claro' : 'Ativar modo escuro');
      button.setAttribute('aria-pressed', String(dark));
    });
    syncInternalLinks(theme);
  }

  const initialized = root.dataset.theme;
  apply(initialized === 'dark' || initialized === 'light' ? initialized : themeFromUrl === 'dark' || themeFromUrl === 'light' ? themeFromUrl : saved || preferred);
  document.addEventListener('click', event => {
    if (!event.target.closest('.theme-toggle')) return;
    const next = root.dataset.theme === 'dark' ? 'light' : 'dark';
    try { localStorage.setItem('almeida-theme', next); } catch (_) { /* propagado pelos links abaixo */ }
    apply(next);
  });
})();
