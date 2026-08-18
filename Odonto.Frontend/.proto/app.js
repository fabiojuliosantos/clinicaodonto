const views = {
  login: document.querySelector('#login-view'),
  register: document.querySelector('#register-view')
};

function showView(name) {
  Object.entries(views).forEach(([key, element]) => {
    const active = key === name;
    element.classList.toggle('active', active);
    element.setAttribute('aria-hidden', String(!active));
  });
  document.title = `${name === 'login' ? 'Entrar' : 'Criar conta'} — Almeida`;
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

document.querySelectorAll('[data-view]').forEach(button => {
  button.addEventListener('click', () => showView(button.dataset.view));
});

document.querySelectorAll('.password-toggle').forEach(button => {
  button.addEventListener('click', () => {
    const input = button.parentElement.querySelector('input');
    const visible = input.type === 'password';
    input.type = visible ? 'text' : 'password';
    button.classList.toggle('visible', visible);
    button.setAttribute('aria-label', visible ? 'Ocultar senha' : 'Mostrar senha');
  });
});

const newPassword = document.querySelector('#register-form input[name="password"]');
newPassword.addEventListener('input', () => {
  const value = newPassword.value;
  let level = 0;
  if (value.length >= 8) level++;
  if (/[A-Z]/.test(value) && /[a-z]/.test(value)) level++;
  if (/\d/.test(value)) level++;
  if (/[^A-Za-z0-9]/.test(value)) level++;
  newPassword.closest('.field').querySelector('.strength').dataset.level = level;
});

function notify(message) {
  const toast = document.querySelector('.toast');
  toast.querySelector('p').textContent = message;
  toast.classList.add('show');
  clearTimeout(notify.timer);
  notify.timer = setTimeout(() => toast.classList.remove('show'), 3800);
}

function validateForm(form) {
  let valid = true;
  form.querySelectorAll('input[required]').forEach(input => {
    let inputValid = input.checkValidity();
    if (input.name === 'emailConfirmation') {
      const email = form.querySelector('input[name="email"]');
      inputValid = input.checkValidity() && input.value.trim().toLowerCase() === email.value.trim().toLowerCase();
    }
    const field = input.closest('.field');
    if (field) field.classList.toggle('invalid', !inputValid);
    if (!inputValid) valid = false;
  });
  return valid;
}

document.querySelectorAll('form').forEach(form => {
  form.addEventListener('input', event => event.target.closest('.field')?.classList.remove('invalid'));
  form.addEventListener('submit', event => {
    event.preventDefault();
    if (!validateForm(form)) {
      form.querySelector('.invalid input')?.focus();
      return;
    }
    if (form.id === 'login-form') {
      notify('Acesso validado. Abrindo o painel...');
      setTimeout(() => { window.location.href = 'dashboard.html'; }, 700);
    }
    else {
      notify('Solicitação enviada! Aguarde a aprovação do administrador.');
      form.reset();
      setTimeout(() => showView('login'), 1200);
    }
  });
});

document.querySelector('#forgot-button').addEventListener('click', () => {
  notify('Um link de recuperação seria enviado ao seu e-mail.');
});
