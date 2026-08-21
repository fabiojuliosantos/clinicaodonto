document.body.classList.add('grid-legibility');

const entityRoot = document.createElement('div');
entityRoot.className = 'entity-backdrop';
entityRoot.id = 'entity-backdrop';
entityRoot.innerHTML = '<div class="entity-dialog" role="dialog" aria-modal="true" aria-labelledby="entity-title"><div id="entity-content"></div></div>';
document.body.appendChild(entityRoot);

const patientOptions = ['João da Silva','Ana Carolina','Rafael Martins','Camila Barbosa','Lucas Pereira','Mariana Souza'];

function modalTemplate(type) {
  const patient = type === 'patient';
  const finance = type === 'finance';
  const team = type === 'team';
  const inventory = type === 'inventory';
  const stockMovement = type === 'stock-movement';
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
    </div></div>` : finance ? `
    <div class="entity-section"><h3>Dados do lançamento</h3><div class="entity-form-grid">
      <label class="entity-field full">Descrição *<input required placeholder="Ex.: Restauração em resina"></label>
      <label class="entity-field">Tipo *<select required><option>Receita</option><option>Despesa</option></select></label>
      <label class="entity-field">Valor *<input type="number" min="0.01" step="0.01" required placeholder="0,00"></label>
      <label class="entity-field">Vencimento *<input type="date" required></label>
      <label class="entity-field">Situação<select><option>A receber</option><option>Recebido</option><option>Agendado</option></select></label>
      <label class="entity-field full">Paciente ou categoria<input placeholder="Vincule quando aplicável"></label>
      <label class="entity-field full">Observações<textarea placeholder="Informações úteis para a equipe financeira"></textarea></label>
    </div></div>` : team ? `
    <div class="entity-section"><h3>Dados do colaborador</h3><div class="entity-form-grid">
      <label class="entity-field full">Nome completo *<input required autocomplete="name" placeholder="Nome e sobrenome"></label>
      <label class="entity-field full">E-mail profissional *<input type="email" required autocomplete="email" placeholder="colaborador@almeida.com.br"></label>
      <label class="entity-field">Função *<select required><option value="">Selecione</option><option>Dentista</option><option>Recepcionista</option><option>Auxiliar</option><option>Administrador</option><option>TI</option></select></label>
      <label class="entity-field">Telefone<input type="tel" placeholder="(81) 99999-9999"></label>
    </div></div>
    <div class="entity-section"><h3>Acesso</h3><p>O convite cria o acesso inicial. As permissões devem ser validadas pelo backend.</p></div>` : inventory ? `
    <div class="entity-section"><h3>Identificação do item</h3><div class="entity-form-grid">
      <label class="entity-field full">Nome do item *<input required placeholder="Ex.: Resina composta A2"></label>
      <label class="entity-field">Código interno *<input required placeholder="Ex.: RES-001"></label>
      <label class="entity-field">Categoria *<select required><option value="">Selecione</option><option>Anestésicos</option><option>Cimentação</option><option>Descartáveis</option><option>Restauradores</option><option>Outros</option></select></label>
      <label class="entity-field">Unidade de controle *<input required placeholder="Ex.: caixas, frascos, unidades"></label>
      <label class="entity-field">Estoque mínimo *<input type="number" min="0" required value="0"></label>
    </div></div>
    <div class="entity-section"><h3>Lote e fornecimento inicial</h3><div class="entity-form-grid">
      <label class="entity-field">Lote<input placeholder="Identificação do fabricante"></label>
      <label class="entity-field">Validade<input type="date"></label>
      <label class="entity-field full">Fornecedor<input placeholder="Nome do fornecedor"></label>
    </div></div>` : stockMovement ? `
    <div class="entity-section"><h3>Movimentação</h3><div class="entity-form-grid">
      <label class="entity-field full">Item *<select required><option value="">Selecione um item</option><option>Resina composta A2</option><option>Lidocaína 2% com epinefrina</option><option>Luva de procedimento M</option><option>Adesivo universal</option></select></label>
      <label class="entity-field">Tipo *<select required><option>Entrada</option><option>Saída</option><option>Ajuste de inventário</option></select></label>
      <label class="entity-field">Quantidade *<input type="number" min="0.01" step="0.01" required></label>
      <label class="entity-field">Lote<input placeholder="Lote movimentado"></label>
      <label class="entity-field">Data *<input type="date" required value="2026-08-19"></label>
      <label class="entity-field full">Motivo / observação *<textarea required placeholder="Informe o motivo da movimentação"></textarea></label>
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
  const labels = patient ? ['Novo paciente','Cadastre as informações essenciais do paciente.','Salvar paciente','Paciente cadastrado','O cadastro já está disponível na lista de pacientes.'] : finance ? ['Novo lançamento','Registre a movimentação sem sair do financeiro.','Salvar lançamento','Lançamento salvo','A movimentação foi incluída no financeiro.'] : team ? ['Convidar colaborador','Cadastre somente os dados necessários para o acesso interno.','Enviar convite','Convite preparado','O colaborador receberá as orientações de acesso.'] : inventory ? ['Novo item de estoque','Cadastre o insumo e seus dados de rastreabilidade.','Salvar item','Item cadastrado','O item já está disponível no controle de estoque.'] : stockMovement ? ['Registrar movimentação','Informe a entrada, saída ou ajuste realizado.','Registrar movimentação','Movimentação registrada','O saldo e o histórico do item foram atualizados.'] : ['Novo agendamento','Reserve um horário sem sair da agenda.','Criar agendamento','Agendamento criado','O horário foi incluído como aguardando confirmação.'];
  return `<form id="entity-form"><header class="entity-header"><div class="entity-heading"><span>${patient?'♙':inventory?'□':stockMovement?'↕':'＋'}</span><div><h2 id="entity-title">${labels[0]}</h2><p>${labels[1]}</p></div></div><button type="button" class="entity-close" aria-label="Fechar">×</button></header><div class="entity-body">${fields}</div><footer class="entity-footer"><button type="button" class="entity-cancel">Cancelar</button><button class="entity-submit">${labels[2]}</button></footer></form><div class="entity-success"><span>✓</span><h3>${labels[3]}</h3><p>${labels[4]}</p></div>`;
}

function openEntityModal(type) {
  entityRoot.querySelector('#entity-content').innerHTML = modalTemplate(type);
  entityRoot.classList.add('open');
  document.body.style.overflow = 'hidden';
  setTimeout(()=>entityRoot.querySelector('input,select')?.focus(),100);
}
function closeEntityModal() { entityRoot.classList.remove('open'); document.body.style.overflow=''; }

document.addEventListener('click', event => {
  const entityButton = event.target.closest('[data-entity]');
  if (entityButton) { event.preventDefault(); openEntityModal(entityButton.dataset.entity); return; }
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
  const destination = directPage==='novo-paciente'?'modulo.html?page=pacientes':'modulo.html?page=agenda';
  const selectedTheme = new URLSearchParams(location.search).get('theme');
  history.replaceState({},'',selectedTheme ? `${destination}&theme=${selectedTheme}` : destination);
  openEntityModal(directPage==='novo-paciente'?'patient':'appointment');
}
