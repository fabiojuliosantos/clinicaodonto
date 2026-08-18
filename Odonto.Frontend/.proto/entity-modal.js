document.body.classList.add('grid-legibility');

const entityRoot = document.createElement('div');
entityRoot.className = 'entity-backdrop';
entityRoot.id = 'entity-backdrop';
entityRoot.innerHTML = '<div class="entity-dialog" role="dialog" aria-modal="true" aria-labelledby="entity-title"><div id="entity-content"></div></div>';
document.body.appendChild(entityRoot);

const patientOptions = ['João da Silva','Ana Carolina','Rafael Martins','Camila Barbosa','Lucas Pereira','Mariana Souza'];

function modalTemplate(type) {
  const patient = type === 'patient';
  const fields = patient ? `
    <div class="entity-section"><h3>Dados pessoais</h3><div class="entity-form-grid">
      <label class="entity-field full">Nome completo *<input name="name" required placeholder="Nome e sobrenome"></label>
      <label class="entity-field">CPF *<input name="cpf" required placeholder="000.000.000-00"></label>
      <label class="entity-field">Data de nascimento *<input type="date" required></label>
    </div></div>
    <div class="entity-section"><h3>Contato</h3><div class="entity-form-grid">
      <label class="entity-field">WhatsApp *<input name="phone" required placeholder="(81) 99999-9999"></label>
      <label class="entity-field">E-mail<input type="email" placeholder="paciente@email.com"></label>
      <label class="entity-field full">Observações<textarea placeholder="Informações importantes para a equipe"></textarea></label>
    </div></div>` : `
    <div class="entity-section"><h3>Atendimento</h3><div class="entity-form-grid">
      <label class="entity-field full">Paciente *<select required><option value="">Selecione um paciente</option>${patientOptions.map(name=>`<option>${name}</option>`).join('')}</select></label>
      <label class="entity-field">Procedimento *<select required><option value="">Selecione</option><option>Avaliação inicial</option><option>Limpeza</option><option>Restauração</option><option>Clareamento</option><option>Retorno</option></select></label>
      <label class="entity-field">Profissional *<select required><option>Dra. Almeida</option><option>Dr. Marcelo</option></select></label>
    </div></div>
    <div class="entity-section"><h3>Data e horário</h3><div class="entity-form-grid">
      <label class="entity-field">Data *<input type="date" required value="2026-08-18"></label>
      <label class="entity-field">Horário *<input type="time" required value="10:00"></label>
      <label class="entity-field">Duração<select><option>30 minutos</option><option selected>45 minutos</option><option>60 minutos</option></select></label>
      <label class="entity-field">Status<select><option>Aguardando confirmação</option><option>Confirmado</option></select></label>
      <label class="entity-field full">Observações<textarea placeholder="Recado para a equipe"></textarea></label>
    </div></div>`;
  return `<form id="entity-form"><header class="entity-header"><div class="entity-heading"><span>${patient?'♙':'＋'}</span><div><h2 id="entity-title">${patient?'Novo paciente':'Novo agendamento'}</h2><p>${patient?'Cadastre as informações essenciais do paciente.':'Reserve um horário sem sair da tela atual.'}</p></div></div><button type="button" class="entity-close" aria-label="Fechar">×</button></header><div class="entity-body">${fields}</div><footer class="entity-footer"><button type="button" class="entity-cancel">Cancelar</button><button class="entity-submit">${patient?'Salvar paciente':'Criar agendamento'}</button></footer></form><div class="entity-success"><span>✓</span><h3>${patient?'Paciente cadastrado':'Agendamento criado'}</h3><p>${patient?'O novo cadastro já está disponível na lista de pacientes.':'O horário foi incluído na agenda como aguardando confirmação.'}</p></div>`;
}

function openEntityModal(type) {
  entityRoot.querySelector('#entity-content').innerHTML = modalTemplate(type);
  entityRoot.classList.add('open');
  document.body.style.overflow = 'hidden';
  setTimeout(()=>entityRoot.querySelector('input,select')?.focus(),100);
}
function closeEntityModal() { entityRoot.classList.remove('open'); document.body.style.overflow=''; }

document.addEventListener('click', event => {
  const createLink = event.target.closest('a[href*="page=novo-paciente"],a[href*="page=novo-agendamento"]');
  if (createLink) { event.preventDefault(); openEntityModal(createLink.href.includes('novo-paciente')?'patient':'appointment'); return; }
  if (event.target.closest('.entity-close,.entity-cancel')) closeEntityModal();
});
entityRoot.addEventListener('click', event => { if(event.target===entityRoot) closeEntityModal(); });
entityRoot.addEventListener('submit', event => {
  event.preventDefault();
  if(!event.target.checkValidity()){event.target.reportValidity();return;}
  event.target.style.display='none';
  entityRoot.querySelector('.entity-success').classList.add('show');
  setTimeout(closeEntityModal,1400);
});
document.addEventListener('keydown',event=>{if(event.key==='Escape'&&entityRoot.classList.contains('open'))closeEntityModal();});

const directPage = new URLSearchParams(location.search).get('page');
if(directPage==='novo-paciente'||directPage==='novo-agendamento') {
  history.replaceState({},'',directPage==='novo-paciente'?'modulo.html?page=pacientes':'modulo.html?page=agenda');
  openEntityModal(directPage==='novo-paciente'?'patient':'appointment');
}
