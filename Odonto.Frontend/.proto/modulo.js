const navigationIconPaths = {
  grid: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',
  calendar: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M8 3v4M16 3v4M3 10h18"/>',
  users: '<path d="M16 21v-2a4 4 0 00-4-4H6a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM22 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"/>',
  tooth: '<path d="M12 5C8 1 3 3 3 8c0 4 2 5 3 10 .5 2 2 3 3 0l1-4c.5-2 3-2 4 0l1 4c1 3 2.5 2 3 0 1-5 3-6 3-10 0-5-5-7-9-3z"/>',
  file: '<path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6M8 13h8M8 17h6"/>',
  wallet: '<path d="M20 7V5a2 2 0 00-2-2H5a3 3 0 000 6h15v11H5a3 3 0 01-3-3V6"/><path d="M16 13h2"/>',
  package: '<path d="M21 16V8a2 2 0 00-1-1.73l-7-4a2 2 0 00-2 0l-7 4A2 2 0 003 8v8a2 2 0 001 1.73l7 4a2 2 0 002 0l7-4A2 2 0 0021 16z"/><path d="M3.3 7 12 12l8.7-5M12 22V12"/>',
  chart: '<path d="M3 3v18h18M7 16l4-5 3 3 5-7"/>',
  team: '<path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM19 8v6M22 11h-6"/>',
  settings: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 00.34 1.88l.06.06-2.83 2.83-.06-.06a1.7 1.7 0 00-1.88-.34 1.7 1.7 0 00-1 1.55V21h-4v-.09A1.7 1.7 0 009 19.37a1.7 1.7 0 00-1.88.34l-.06.06-2.83-2.83.06-.06A1.7 1.7 0 004.63 15a1.7 1.7 0 00-1.55-1H3v-4h.09A1.7 1.7 0 004.63 9a1.7 1.7 0 00-.34-1.88l-.06-.06 2.83-2.83.06.06A1.7 1.7 0 009 4.63h.01A1.7 1.7 0 0010 3.08V3h4v.09A1.7 1.7 0 0015 4.63a1.7 1.7 0 001.88-.34l.06-.06 2.83 2.83-.06.06A1.7 1.7 0 0019.37 9v.01A1.7 1.7 0 0020.92 10H21v4h-.09A1.7 1.7 0 0019.4 15z"/>'
};
document.querySelectorAll('.main-nav [data-icon]').forEach(element => {
  const path = navigationIconPaths[element.dataset.icon];
  if (path) element.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.15" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${path}</svg>`;
});

const params = new URLSearchParams(window.location.search);
const requestedPage = params.get('page') || 'agenda';
const page = requestedPage === 'novo-paciente' ? 'pacientes' : requestedPage === 'novo-agendamento' ? 'agenda' : requestedPage;
const content = document.querySelector('#module-content');

const patients = [
  ['JS','João da Silva','(81) 99945-2210','joao@email.com','Hoje, 08:00','Ativo','pastel-blue','joao'],
  ['AC','Ana Carolina','(81) 98876-1032','ana@email.com','Hoje, 09:00','Em tratamento','pastel-pink','ana'],
  ['RM','Rafael Martins','(81) 99712-4580','rafael@email.com','Hoje, 10:30','Ativo','pastel-yellow','rafael'],
  ['CB','Camila Barbosa','(81) 98645-7731','camila@email.com','Hoje, 11:30','Novo','pastel-purple','camila'],
  ['LP','Lucas Pereira','(81) 99884-1205','lucas@email.com','Hoje, 14:00','Em tratamento','pastel-green','lucas'],
  ['MS','Mariana Souza','(81) 98774-3098','mariana@email.com','12 ago. 2026','Pendente','pastel-pink','mariana']
];

const header = (title, description, actions = '', parent = '') => `
  <header class="module-head">
    <div><div class="breadcrumb"><a href="dashboard.html">Visão geral</a>${parent ? `<span>›</span><a href="modulo.html?page=${parent}">${labelFor(parent)}</a>` : ''}<span>›</span><span>${title}</span></div><h1>${title}</h1><p>${description}</p></div>
    <div class="head-actions">${actions}</div>
  </header>`;

const labelFor = value => ({agenda:'Agenda',pacientes:'Pacientes',atendimentos:'Atendimentos',prontuarios:'Prontuários',financeiro:'Financeiro',estoque:'Estoque',relatorios:'Relatórios',equipe:'Equipe',configuracoes:'Configurações'})[value] || value;
const card = body => `<section class="content-card">${body}</section>`;
const person = (p, link = true) => `<div class="person-cell"><div class="patient-avatar ${p[6]}">${p[0]}</div><div>${link ? `<a class="table-link" href="modulo.html?page=paciente&id=${p[7]}"><strong>${p[1]}</strong></a>` : `<strong>${p[1]}</strong>`}<small>${p[2]}</small></div></div>`;

function agendaPage() {
  const hours = ['08:00','09:00','10:00','11:00','12:00','13:00','14:00','15:00','16:00','17:00'];
  const events = {
    '08:00-0':'<div class="calendar-event"><strong>João da Silva</strong>Limpeza · 45 min</div>',
    '09:00-1':'<div class="calendar-event green-event"><strong>Ana Carolina</strong>Clareamento · 60 min</div>',
    '10:00-2':'<div class="calendar-event"><strong>Rafael Martins</strong>Restauração · 45 min</div>',
    '11:00-3':'<div class="calendar-event amber-event"><strong>Camila Barbosa</strong>Avaliação · pendente</div>',
    '14:00-1':'<div class="calendar-event green-event"><strong>Lucas Pereira</strong>Canal · 60 min</div>',
    '15:00-4':'<div class="calendar-event"><strong>Beatriz Lima</strong>Retorno · 30 min</div>'
  };
  const rows = hours.map(hour => `<div class="calendar-row"><time>${hour}</time>${Array.from({length:5},(_,i)=>`<div>${events[`${hour}-${i}`] || ''}</div>`).join('')}</div>`).join('');
  content.innerHTML = header('Agenda','Organize os horários da equipe e acompanhe o fluxo do dia.',`<a href="modulo.html?page=novo-agendamento" class="action-primary">＋ Novo agendamento</a>`) + `
    <div class="toolbar"><div class="segmented"><button>Dia</button><button class="active">Semana</button><button>Mês</button></div><div class="toolbar-group"><button class="select-soft">‹</button><button class="select-soft">Hoje</button><button class="select-soft">›</button><select class="select-soft"><option>Todos os status</option><option>Confirmadas</option><option>Pendentes</option></select></div></div>
    ${card(`<div class="calendar-layout"><aside class="calendar-side"><div class="mini-calendar"><h3>Agosto 2026</h3><div class="mini-grid">${['D','S','T','Q','Q','S','S',...Array.from({length:31},(_,i)=>i+1)].map((d,i)=>`<span class="${i===22?'today':''}">${d}</span>`).join('')}</div></div><div class="professional-filter"><h3>Profissionais</h3><label class="professional-option"><i></i><input type="checkbox" checked>Dra. Almeida</label><label class="professional-option"><i></i><input type="checkbox" checked>Dr. Marcelo</label></div></aside><div class="calendar-main"><div class="calendar-header"><div></div>${['Seg 17','Ter 18','Qua 19','Qui 20','Sex 21'].map((d,i)=>`<div><span>${d.split(' ')[0]}</span><strong>${d.split(' ')[1]}</strong></div>`).join('')}</div>${rows}</div></div>`)} `;
}

function patientsPage() {
  content.innerHTML = header('Pacientes','Cadastros, contatos e histórico clínico em um só lugar.',`<a href="modulo.html?page=novo-paciente" class="action-primary">＋ Novo paciente</a>`) + `
    <div class="toolbar"><input class="filter-input" id="table-search" placeholder="Buscar por nome, CPF ou telefone"><div class="toolbar-group"><select class="select-soft" id="status-filter"><option>Todos os pacientes</option><option>Ativo</option><option>Em tratamento</option><option>Novo</option><option>Pendente</option></select><button class="select-soft" data-toast="Lista exportada em CSV.">⇩ Exportar</button></div></div>
    ${card(`<table class="data-table"><thead><tr><th>Paciente</th><th>Contato</th><th>Última consulta</th><th>Situação</th><th></th></tr></thead><tbody id="patients-body">${patients.map(p=>`<tr data-name="${p[1].toLowerCase()}" data-status="${p[5]}"><td>${person(p)}</td><td>${p[3]}</td><td>${p[4]}</td><td><span class="badge ${p[5]==='Pendente'?'amber':p[5]==='Novo'?'blue':'green'}">${p[5]}</span></td><td><a class="table-link" href="modulo.html?page=paciente&id=${p[7]}">Abrir →</a></td></tr>`).join('')}</tbody></table>`)} `;
}

function patientFormPage() {
  content.innerHTML = header('Novo paciente','Cadastre os dados pessoais e informações de contato.','', 'pacientes') + card(`<form class="form-card prototype-form" data-success="Paciente cadastrado com sucesso." data-redirect="modulo.html?page=paciente&id=novo"><div class="form-section"><h2>Dados pessoais</h2><p>Informações principais para identificação do paciente.</p><div class="soft-form"><label>Nome completo *<input name="name" required placeholder="Nome e sobrenome"></label><label>CPF *<input name="cpf" required placeholder="000.000.000-00"></label><label>Data de nascimento *<input type="date" required></label><label>Gênero<select><option value="">Selecione</option><option>Feminino</option><option>Masculino</option><option>Prefiro não informar</option></select></label></div></div><div class="form-section"><h2>Contato</h2><p>Dados utilizados pela recepção para confirmações e retornos.</p><div class="soft-form"><label>WhatsApp *<input name="phone" required placeholder="(81) 99999-9999"></label><label>E-mail<input type="email" placeholder="paciente@email.com"></label><label class="full">Endereço<input placeholder="Rua, número, bairro e cidade"></label></div></div><div class="form-section"><h2>Informações complementares</h2><div class="soft-form"><label>Contato de emergência<input placeholder="Nome e telefone"></label><label>Profissional responsável<select><option>Dra. Almeida</option><option>Dr. Marcelo</option></select></label><label class="full">Observações<textarea placeholder="Informações importantes para a equipe"></textarea></label></div></div><div class="form-actions"><a href="modulo.html?page=pacientes">Cancelar</a><button class="action-primary">Salvar paciente</button></div></form>`);
}

function appointmentFormPage() {
  content.innerHTML = header('Novo agendamento','Reserve um horário na agenda da clínica.','', 'agenda') + card(`<form class="form-card prototype-form" data-success="Agendamento criado e marcado como pendente." data-redirect="modulo.html?page=agenda"><div class="form-section"><h2>Paciente e atendimento</h2><p>Selecione quem será atendido e o motivo da consulta.</p><div class="soft-form"><label>Paciente *<select required><option value="">Selecione um paciente</option>${patients.map(p=>`<option>${p[1]}</option>`).join('')}</select></label><label>Procedimento *<select required><option value="">Selecione</option><option>Avaliação inicial</option><option>Limpeza</option><option>Restauração</option><option>Clareamento</option><option>Retorno</option></select></label><label>Profissional *<select required><option>Dra. Almeida</option><option>Dr. Marcelo</option></select></label><label>Duração<select><option>30 minutos</option><option selected>45 minutos</option><option>60 minutos</option><option>90 minutos</option></select></label></div></div><div class="form-section"><h2>Data e horário</h2><div class="soft-form"><label>Data *<input type="date" required value="2026-08-18"></label><label>Horário *<input type="time" required value="10:00"></label><label class="full">Observações<textarea placeholder="Recado para o profissional ou para a recepção"></textarea></label></div></div><div class="form-actions"><a href="modulo.html?page=agenda">Cancelar</a><button class="action-primary">Confirmar agendamento</button></div></form>`);
}

function patientPage() {
  const id = params.get('id') || 'rafael';
  const found = patients.find(p=>p[7]===id) || ['NP','Novo Paciente','(81) 99999-9999','novo@email.com','Ainda não atendido','Novo','pastel-blue','novo'];
  content.innerHTML = header(found[1],'Ficha central do paciente, histórico e informações clínicas.',`<a href="modulo.html?page=novo-agendamento" class="action-primary">＋ Agendar consulta</a>`,'pacientes') + card(`<div class="patient-hero"><div class="patient-identity"><div class="patient-avatar ${found[6]}">${found[0]}</div><div><h2>${found[1]}</h2><p>CPF 124.***.***-08 · 34 anos · Paciente desde março de 2024</p></div></div><div class="patient-quick"><div><small>WhatsApp</small><strong>${found[2]}</strong></div><div><small>Próxima consulta</small><strong>Hoje, 10:30</strong></div><div><small>Situação</small><strong>Em tratamento</strong></div></div></div><nav class="tabs"><a class="active" href="#resumo">Resumo</a><a href="modulo.html?page=prontuario&id=${id}">Prontuário</a><a href="modulo.html?page=odontograma&id=${id}">Odontograma</a><a href="modulo.html?page=financeiro&paciente=${id}">Financeiro</a><a href="#documentos">Documentos</a></nav>`) + `<div class="detail-grid"><section class="content-card detail-panel"><h2>Histórico recente</h2><div class="timeline"><div class="timeline-item"><time>12 JUN. 2026 · DRA. ALMEIDA</time><h3>Restauração em resina — dente 26</h3><p>Procedimento concluído sem intercorrências. Paciente orientado sobre sensibilidade nas primeiras 24 horas.</p></div><div class="timeline-item"><time>28 MAI. 2026 · DRA. ALMEIDA</time><h3>Avaliação e plano de tratamento</h3><p>Exame clínico realizado e orçamento aprovado pelo paciente.</p></div><div class="timeline-item"><time>05 MAR. 2024 · RECEPÇÃO</time><h3>Cadastro do paciente</h3><p>Cadastro realizado por Julia Guerra de Almeida.</p></div></div><a class="table-link" href="modulo.html?page=prontuario&id=${id}">Ver prontuário completo →</a></section><aside class="content-card detail-panel"><h2>Informações importantes</h2><div class="info-list"><div class="info-row"><span>Profissional</span><strong>Dra. Almeida</strong></div><div class="info-row"><span>Última consulta</span><strong>12 jun. 2026</strong></div><div class="info-row"><span>Plano atual</span><strong>4 de 6 realizados</strong></div><div class="info-row"><span>Saldo pendente</span><strong>R$ 420,00</strong></div></div><div class="clinical-alert"><strong>⚠ Alerta clínico</strong><br>Alergia a dipirona registrada na anamnese.</div></aside></div>`;
}

function appointmentsPage() {
  content.innerHTML = header('Atendimentos','Acompanhe a fila clínica e registre a evolução dos pacientes.',`<a href="modulo.html?page=agenda">Abrir agenda</a>`) + `<div class="toolbar"><div class="segmented"><button class="active">Hoje</button><button>Em andamento</button><button>Concluídos</button></div></div>` + card(`<table class="data-table"><thead><tr><th>Horário / paciente</th><th>Procedimento</th><th>Profissional</th><th>Status</th><th></th></tr></thead><tbody>${patients.slice(0,5).map((p,i)=>`<tr><td>${person(p)}</td><td>${['Limpeza e avaliação','Clareamento','Restauração','Avaliação inicial','Tratamento de canal'][i]}</td><td>${i%2?'Dr. Marcelo':'Dra. Almeida'}</td><td><span class="badge ${i<2?'green':i===2?'blue':'gray'}">${i<2?'Concluído':i===2?'Próximo':'Agendado'}</span></td><td><a class="table-link" href="modulo.html?page=atendimento&id=${p[7]}">${i<2?'Revisar':'Iniciar'} →</a></td></tr>`).join('')}</tbody></table>`);
}

function clinicalPage(mode = 'atendimento') {
  const title = mode === 'prontuario' ? 'Prontuário de Rafael Martins' : 'Atendimento — Rafael Martins';
  content.innerHTML = header(title,'Registro clínico protegido e vinculado ao profissional responsável.',`<a href="modulo.html?page=paciente&id=rafael">Ver ficha do paciente</a>`,'atendimentos') + `<div class="detail-grid"><section class="content-card form-card"><div class="form-section"><h2>Evolução clínica</h2><p>Descreva avaliação, conduta e orientações dadas ao paciente.</p><form class="soft-form prototype-form" data-success="Evolução salva no prontuário."><label class="full">Procedimento<select><option>Restauração em resina</option><option>Avaliação</option><option>Limpeza</option></select></label><label class="full">Registro clínico<textarea required placeholder="Registre os detalhes do atendimento...">Paciente compareceu sem queixas de dor. Realizada avaliação do elemento 26 e preparo para restauração.</textarea></label><label>Dente / região<input value="26"></label><label>Próximo retorno<input type="date" value="2026-09-17"></label><label class="full">Anexos<input type="file" multiple></label><div class="form-actions full"><button type="button" data-toast="Rascunho salvo.">Salvar rascunho</button><button class="action-primary">Finalizar e assinar</button></div></form></div></section><aside class="content-card detail-panel"><h2>Resumo do paciente</h2><div class="patient-identity"><div class="patient-avatar pastel-yellow">RM</div><div><h2>Rafael Martins</h2><p>34 anos · Prontuário #00248</p></div></div><div class="clinical-alert"><strong>⚠ Alergia</strong><br>Dipirona</div><div class="info-list" style="margin-top:16px"><div class="info-row"><span>Pressão arterial</span><strong>120 × 80</strong></div><div class="info-row"><span>Medicamento</span><strong>Nenhum</strong></div><div class="info-row"><span>Última consulta</span><strong>12 jun. 2026</strong></div></div><a class="table-link" href="modulo.html?page=odontograma&id=rafael">Abrir odontograma →</a></aside></div>`;
}

function recordsPage() {
  content.innerHTML = header('Prontuários','Consulte registros clínicos, anamneses e documentos.',`<a href="modulo.html?page=atendimentos">Atendimentos de hoje</a>`) + `<div class="toolbar"><input class="filter-input" id="table-search" placeholder="Buscar prontuário ou paciente"><select class="select-soft"><option>Atualizados recentemente</option><option>Ordem alfabética</option></select></div>` + card(`<table class="data-table"><thead><tr><th>Paciente</th><th>Último registro</th><th>Profissional</th><th>Anamnese</th><th></th></tr></thead><tbody id="patients-body">${patients.map((p,i)=>`<tr data-name="${p[1].toLowerCase()}"><td>${person(p)}</td><td>${i<3?'Hoje':`${12-i} ago. 2026`}</td><td>${i%2?'Dr. Marcelo':'Dra. Almeida'}</td><td><span class="badge ${i===3?'amber':'green'}">${i===3?'Revisar':'Atualizada'}</span></td><td><a class="table-link" href="modulo.html?page=prontuario&id=${p[7]}">Abrir →</a></td></tr>`).join('')}</tbody></table>`);
}

function odontogramPage() {
  content.innerHTML = header('Odontograma — Rafael Martins','Selecione um dente para registrar sua condição ou procedimento.',`<a href="modulo.html?page=paciente&id=rafael">Voltar à ficha</a>`,'prontuarios') + card(`<div class="form-card"><div class="toolbar"><div><strong style="font-size:12px">Arcada permanente</strong><p style="font-size:9px;color:var(--muted)">Clique em um elemento para editar.</p></div><div class="toolbar-group"><span class="badge blue">Planejado</span><span class="badge green">Realizado</span><span class="badge amber">Atenção</span></div></div><div class="odontogram">${[18,17,16,15,14,13,12,11,21,22,23,24,25,26,27,28,48,47,46,45,44,43,42,41,31,32,33,34,35,36,37,38].map(n=>`<button class="tooth-item ${n===26?'selected':''}" data-tooth="${n}">${n}</button>`).join('')}</div><div class="form-section"><h2 id="tooth-title">Dente 26</h2><div class="soft-form"><label>Condição<select><option>Restauração</option><option>Saudável</option><option>Cárie</option><option>Ausente</option><option>Implante</option></select></label><label>Situação<select><option>Realizado</option><option>Planejado</option><option>Em acompanhamento</option></select></label><label class="full">Observação<textarea>Restauração em resina realizada em 12/06/2026.</textarea></label></div></div><div class="form-actions"><button data-toast="Alteração registrada no histórico do odontograma." class="action-primary">Salvar alteração</button></div></div>`);
}

function financePage() {
  content.innerHTML = header('Financeiro','Movimentações, recebimentos e saúde financeira da clínica.',`<button data-toast="Lançamento financeiro criado.">＋ Novo lançamento</button>`) + `<section class="finance-metrics"><article class="content-card finance-card"><span>Receita no mês</span><strong>R$ 28.460</strong><small>↑ 8,4% sobre julho</small></article><article class="content-card finance-card"><span>A receber</span><strong>R$ 7.280</strong><small>18 parcelas abertas</small></article><article class="content-card finance-card"><span>Despesas</span><strong>R$ 9.840</strong><small>34,5% da receita</small></article><article class="content-card finance-card"><span>Em atraso</span><strong>R$ 1.260</strong><small style="color:#b77878">5 cobranças pendentes</small></article></section><div class="toolbar"><div class="segmented"><button class="active">Movimentações</button><button>Contas a receber</button><button>Contas a pagar</button></div><select class="select-soft"><option>Agosto de 2026</option><option>Julho de 2026</option></select></div>` + card(`<table class="data-table"><thead><tr><th>Descrição</th><th>Paciente / categoria</th><th>Vencimento</th><th>Valor</th><th>Status</th></tr></thead><tbody><tr><td><strong>Clareamento · parcela 2/3</strong></td><td>Ana Carolina</td><td>Hoje</td><td>R$ 480,00</td><td><span class="badge green">Recebido</span></td></tr><tr><td><strong>Restauração</strong></td><td>Rafael Martins</td><td>20 ago.</td><td>R$ 420,00</td><td><span class="badge blue">A receber</span></td></tr><tr><td><strong>Material odontológico</strong></td><td>Fornecedor · insumos</td><td>20 ago.</td><td>− R$ 1.240,00</td><td><span class="badge gray">Agendado</span></td></tr><tr><td><strong>Tratamento ortodôntico</strong></td><td>Mariana Souza</td><td>14 ago.</td><td>R$ 360,00</td><td><span class="badge amber">Em atraso</span></td></tr></tbody></table>`);
}

function inventoryPage() {
  const items = [
    ['RES-001','Resina composta A2','Restauradores','18','10','seringas','RC2407','30 jul. 2027','Dental Recife','Adequado','green'],
    ['ANES-004','Lidocaína 2% com epinefrina','Anestésicos','8','12','tubetes','LD2604','18 out. 2026','OdontoMed','Estoque baixo','amber'],
    ['LUV-010','Luva de procedimento M','Descartáveis','6','15','caixas','LV1198','—','SupriClínica','Estoque baixo','amber'],
    ['ADES-002','Adesivo universal','Restauradores','9','6','frascos','AD8821','12 set. 2026','Dental Recife','Validade próxima','blue'],
    ['AGU-006','Agulha gengival curta','Descartáveis','24','10','caixas','AG3402','22 mar. 2028','OdontoMed','Adequado','green'],
    ['CIM-003','Cimento resinoso dual','Cimentação','4','4','kits','CR7710','08 nov. 2026','Dental Recife','No limite','gray']
  ];
  content.innerHTML = header('Estoque','Controle de insumos, lotes, validade e movimentações do consultório.',`<button class="action-primary" data-entity="inventory">＋ Novo item</button><button data-entity="stock-movement">↕ Registrar movimentação</button>`) + `
    <section class="finance-metrics inventory-metrics">
      <article class="content-card finance-card"><span>Itens cadastrados</span><strong>68</strong><small>12 categorias ativas</small></article>
      <article class="content-card finance-card"><span>Abaixo do mínimo</span><strong>2</strong><small class="inventory-warning">Reposição necessária</small></article>
      <article class="content-card finance-card"><span>Validade próxima</span><strong>1</strong><small>Próximos 60 dias</small></article>
      <article class="content-card finance-card"><span>Movimentações no mês</span><strong>47</strong><small>31 saídas · 16 entradas</small></article>
    </section>
    <div class="toolbar"><input class="filter-input" id="table-search" placeholder="Buscar item, categoria, lote ou fornecedor"><div class="toolbar-group"><select class="select-soft" id="status-filter"><option value="">Todos os status</option><option>Estoque baixo</option><option>Validade próxima</option><option>Adequado</option><option>No limite</option></select><select class="select-soft"><option>Todas as categorias</option><option>Anestésicos</option><option>Descartáveis</option><option>Restauradores</option></select></div></div>
    <div class="inventory-layout">
      ${card(`<div class="table-scroll"><table class="data-table inventory-table"><thead><tr><th>Item</th><th>Saldo / mínimo</th><th>Lote</th><th>Validade</th><th>Fornecedor</th><th>Situação</th><th></th></tr></thead><tbody id="patients-body">${items.map(item=>`<tr data-name="${item.join(' ').toLowerCase()}" data-status="${item[9]}"><td><div class="inventory-item"><span>${item[0]}</span><strong>${item[1]}</strong><small>${item[2]}</small></div></td><td><strong>${item[3]} ${item[5]}</strong><small class="stock-minimum">Mín. ${item[4]}</small></td><td>${item[6]}</td><td>${item[7]}</td><td>${item[8]}</td><td><span class="badge ${item[10]}">${item[9]}</span></td><td><button class="table-menu" data-toast="Detalhes de ${item[1]} abertos." aria-label="Abrir detalhes de ${item[1]}">•••</button></td></tr>`).join('')}</tbody></table></div>`)}
      <aside class="content-card inventory-activity"><div class="inventory-panel-head"><div><h2>Movimentações recentes</h2><p>Últimos registros do estoque</p></div><a href="#historico">Ver todas</a></div><ol><li><span class="movement-icon out">−</span><div><strong>Resina composta A2</strong><small>Saída de 1 seringa · Atendimento</small><time>Hoje, 10:42</time></div></li><li><span class="movement-icon in">＋</span><div><strong>Luva de procedimento M</strong><small>Entrada de 4 caixas · Lote LV1198</small><time>Ontem, 16:18</time></div></li><li><span class="movement-icon out">−</span><div><strong>Lidocaína 2%</strong><small>Saída de 2 tubetes · Atendimento</small><time>Ontem, 11:05</time></div></li></ol></aside>
    </div>`;
}

function reportsPage() {
  content.innerHTML = header('Relatórios','Indicadores para acompanhar a operação e apoiar decisões.',`<button data-toast="Relatório exportado em PDF.">⇩ Exportar PDF</button>`) + `<div class="toolbar"><div class="segmented"><button>7 dias</button><button class="active">30 dias</button><button>Este ano</button></div><select class="select-soft"><option>Todos os profissionais</option><option>Dra. Almeida</option><option>Dr. Marcelo</option></select></div><div class="charts-grid"><section class="content-card chart-card"><h2>Receita mensal</h2><p>Evolução dos últimos seis meses</p><div class="bar-chart">${[['Mar',45],['Abr',58],['Mai',51],['Jun',72],['Jul',67],['Ago',88]].map(v=>`<div class="bar" style="height:${v[1]}%" data-label="${v[0]}"></div>`).join('')}</div></section><section class="content-card chart-card"><h2>Ocupação da agenda</h2><p>Distribuição das consultas no período</p><div class="donut-wrap"><div class="donut"><div><strong>82%</strong><small>ocupação</small></div></div></div></section></div><section class="finance-metrics" style="margin-top:16px"><article class="content-card finance-card"><span>Consultas realizadas</span><strong>126</strong><small>↑ 12% no período</small></article><article class="content-card finance-card"><span>Taxa de faltas</span><strong>6,2%</strong><small>↓ 1,4 ponto</small></article><article class="content-card finance-card"><span>Ticket médio</span><strong>R$ 386</strong><small>↑ R$ 24</small></article><article class="content-card finance-card"><span>Novos pacientes</span><strong>18</strong><small>6 por indicação</small></article></section>`;
}

function teamPage() {
  const team = [['JG','Julia Guerra de Almeida','Proprietária','Acesso total','pastel-blue'],['DA','Dra. Almeida','Cirurgiã-dentista','Clínico e financeiro','pastel-purple'],['DM','Dr. Marcelo','Cirurgião-dentista','Clínico','pastel-green'],['CR','Clara Rocha','Recepcionista','Agenda e pacientes','pastel-pink']];
  content.innerHTML = header('Equipe','Usuários internos, funções e permissões de acesso.',`<button data-toast="Convite enviado para o novo colaborador." class="action-primary">＋ Convidar colaborador</button>`) + `<div class="team-grid">${team.map((m,i)=>`<article class="content-card team-card"><div class="patient-avatar ${m[4]}">${m[0]}</div><h3>${m[1]}</h3><p>${m[2]}</p><span class="badge ${i===3?'amber':'green'}">${i===3?'Convite pendente':'Ativo'}</span><footer><span>${m[3]}</span><button class="table-menu" data-toast="Configuração de ${m[1]} aberta.">•••</button></footer></article>`).join('')}</div>`;
}

function settingsPage() {
  content.innerHTML = header('Configurações','Preferências gerais e segurança do sistema.') + `<div class="settings-layout"><nav class="content-card settings-nav"><button class="active" data-settings="clinic">Dados da clínica</button><button data-settings="hours">Horários</button><button data-settings="security">Segurança</button><button data-settings="notifications">Notificações</button><button data-settings="audit">Auditoria</button></nav><section class="content-card settings-panel" id="settings-panel"><h2>Dados da clínica</h2><p style="color:var(--muted);font-size:9px;margin-bottom:20px">Informações utilizadas em documentos e comunicações.</p><form class="soft-form prototype-form" data-success="Configurações salvas."><label class="full">Nome da clínica<input value="Almeida Estética e Sorriso"></label><label>CNPJ<input placeholder="00.000.000/0000-00"></label><label>Telefone<input value="(81) 99999-0000"></label><label class="full">Endereço<input value="Recife, Pernambuco"></label><label class="full">E-mail institucional<input type="email" value="contato@almeida.com.br"></label><div class="form-actions full"><button class="action-primary">Salvar alterações</button></div></form></section></div>`;
}

const renderers = {agenda:agendaPage,pacientes:patientsPage,'novo-paciente':patientFormPage,'novo-agendamento':appointmentFormPage,paciente:patientPage,atendimentos:appointmentsPage,atendimento:()=>clinicalPage('atendimento'),prontuarios:recordsPage,prontuario:()=>clinicalPage('prontuario'),odontograma:odontogramPage,financeiro:financePage,estoque:inventoryPage,relatorios:reportsPage,equipe:teamPage,configuracoes:settingsPage};
(renderers[page] || agendaPage)();

// Cadastros pertencentes a páginas existentes acontecem no próprio contexto.
if (page === 'financeiro') {
  const button = [...document.querySelectorAll('.head-actions button')].find(item => item.textContent.includes('Novo lançamento'));
  if (button) { button.dataset.entity = 'finance'; delete button.dataset.toast; button.classList.add('action-primary'); }
}
if (page === 'equipe') {
  const button = [...document.querySelectorAll('.head-actions button')].find(item => item.textContent.includes('Convidar colaborador'));
  if (button) { button.dataset.entity = 'team'; delete button.dataset.toast; }
}

const navKey = page.startsWith('novo-agendamento') ? 'agenda' : page.startsWith('novo-paciente') || page === 'paciente' ? 'pacientes' : page === 'atendimento' ? 'atendimentos' : ['prontuario','odontograma'].includes(page) ? 'prontuarios' : page;
document.querySelector(`[data-page="${navKey}"]`)?.classList.add('active');
document.title = `${content.querySelector('h1')?.textContent || 'Sistema'} — Almeida`;

const toast = document.querySelector('#module-toast');
function notify(message) { toast.querySelector('p').textContent = message; toast.classList.add('show'); clearTimeout(notify.timer); notify.timer=setTimeout(()=>toast.classList.remove('show'),3000); }
document.addEventListener('click', event => { const trigger=event.target.closest('[data-toast]'); if(trigger){event.preventDefault();notify(trigger.dataset.toast);} });
document.querySelectorAll('.prototype-form').forEach(form=>form.addEventListener('submit',event=>{event.preventDefault();if(!form.checkValidity()){form.reportValidity();return;}notify(form.dataset.success||'Alterações salvas.');if(form.dataset.redirect)setTimeout(()=>location.href=window.withAlmeidaTheme?.(form.dataset.redirect)||form.dataset.redirect,800);}));

const search=document.querySelector('#table-search');
search?.addEventListener('input',()=>document.querySelectorAll('#patients-body tr').forEach(row=>row.hidden=!row.dataset.name.includes(search.value.toLowerCase())));
if(search && params.get('q')) { search.value=params.get('q'); search.dispatchEvent(new Event('input')); }
document.querySelector('#status-filter')?.addEventListener('change',event=>document.querySelectorAll('#patients-body tr').forEach(row=>row.hidden=Boolean(event.target.value)&&row.dataset.status!==event.target.value));
document.querySelectorAll('.segmented button').forEach(button=>button.addEventListener('click',()=>{button.parentElement.querySelectorAll('button').forEach(b=>b.classList.remove('active'));button.classList.add('active');}));
document.querySelectorAll('.tooth-item').forEach(button=>button.addEventListener('click',()=>{document.querySelectorAll('.tooth-item').forEach(b=>b.classList.remove('selected'));button.classList.add('selected');document.querySelector('#tooth-title').textContent=`Dente ${button.dataset.tooth}`;}));
document.querySelectorAll('[data-settings]').forEach(button=>button.addEventListener('click',()=>{document.querySelectorAll('[data-settings]').forEach(b=>b.classList.remove('active'));button.classList.add('active');const titles={clinic:'Dados da clínica',hours:'Horários de funcionamento',security:'Segurança e acesso',notifications:'Notificações',audit:'Auditoria'};document.querySelector('#settings-panel').innerHTML=`<div class="empty-state"><span>✓</span><h2>${titles[button.dataset.settings]}</h2><p>Esta seção demonstra as preferências de ${titles[button.dataset.settings].toLowerCase()}. As configurações serão persistidas pela API na implementação.</p><button class="select-soft" data-toast="Preferência demonstrativa salva.">Salvar preferência</button></div>`;}));

const profileButton=document.querySelector('#profile-button'),profileMenu=document.querySelector('#profile-menu');
profileButton.addEventListener('click',()=>{const open=profileMenu.classList.toggle('show');profileButton.setAttribute('aria-expanded',open);});
document.addEventListener('click',event=>{if(!event.target.closest('.topbar__actions'))profileMenu.classList.remove('show');});
const sidebar=document.querySelector('#sidebar'),overlay=document.querySelector('#mobile-overlay');
const collapseButton=document.querySelector('#collapse-sidebar'),sidebarPreferenceKey='almeida-sidebar-collapsed';
sidebar.querySelectorAll('.main-nav a').forEach(link=>{link.title=link.querySelector(':scope > span:nth-child(2)')?.textContent.trim()||'';});
function setSidebarCollapsed(collapsed){document.documentElement.classList.toggle('sidebar-state-collapsed',collapsed);sidebar.classList.toggle('collapsed',collapsed);collapseButton.setAttribute('aria-expanded',String(!collapsed));const label=collapsed?'Expandir menu':'Recolher menu';collapseButton.setAttribute('aria-label',label);collapseButton.title=label;window.syncAlmeidaSidebarLinks?.(collapsed);}
setSidebarCollapsed(document.documentElement.classList.contains('sidebar-state-collapsed'));
collapseButton.addEventListener('click',()=>{const collapsed=!sidebar.classList.contains('collapsed');setSidebarCollapsed(collapsed);try{localStorage.setItem(sidebarPreferenceKey,String(collapsed));}catch(_){/* estado segue pelos links */}});
document.querySelector('#mobile-menu').addEventListener('click',()=>{sidebar.classList.toggle('open');overlay.classList.toggle('show');});
overlay.addEventListener('click',()=>{sidebar.classList.remove('open');overlay.classList.remove('show');});
document.addEventListener('keydown',event=>{if((event.metaKey||event.ctrlKey)&&event.key.toLowerCase()==='k'){event.preventDefault();document.querySelector('#global-search').focus();}});
document.querySelector('#global-search').addEventListener('keydown',event=>{if(event.key==='Enter'&&event.target.value.trim()){const target=`modulo.html?page=pacientes&q=${encodeURIComponent(event.target.value.trim())}`;location.href=window.withAlmeidaTheme?.(target)||target;}});
