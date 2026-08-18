# Contexto do produto

## Produto

Clínica Odonto é o sistema próprio de gestão interna da Almeida Estética e
Sorriso. Não é um SaaS e não possui área destinada a pacientes ou clientes da
clínica.

A clínica e o sistema ainda estão em implantação. Não há um processo manual ou
sistema legado que precise ser reproduzido. Os fluxos podem ser projetados desde
o início, buscando consistência e eficiência operacional.

## Usuários internos

- Dentistas
- Recepcionistas
- Auxiliares
- Administradores
- TI

Cada perfil terá responsabilidades e privilégios diferentes. A matriz detalhada
de permissões ainda não foi definida e não deve ser presumida durante o
desenvolvimento.

## Escopo funcional da V1

Todos os módulos apresentados em `Odonto.Frontend/.proto/` fazem parte da V1,
incluindo:

- Visão geral (dashboard)
- Agenda
- Pacientes
- Atendimentos
- Prontuários
- Financeiro
- Relatórios
- Equipe
- Configurações

Também fazem parte do produto a gestão do estoque interno, consultas,
procedimentos e documentos clínicos. Os documentos incluem receitas, atestados,
prontuários, termos de consentimento, solicitações de exames e documentos afins.
Funcionalidades adicionais serão discutidas durante a evolução do sistema.

## Primeira etapa

As primeiras entregas são:

1. Cadastro e gestão inicial de funcionários.
2. Login.
3. Dashboard inicial.
4. Base de autorização por perfil, preparada para a futura matriz de permissões.

A implementação inicial deve formar uma base sustentável para a V1, e não um
protótipo descartável.

## Acesso e criação de usuários

Não existe auto cadastro público. Um superusuário de TI será provisionado na
implantação inicial e criará os demais usuários. No futuro, outros usuários
poderão receber essa permissão.

O formulário de criação existente no protótipo deve ser adaptado para o módulo
Equipe. A aba e o fluxo de criação só podem ser exibidos para usuários com a
permissão correspondente, e a API também deve validar essa autorização.

## Princípios de experiência

Por ser uma ferramenta diária de trabalho, a experiência deve priorizar:

- Rapidez e clareza nas tarefas recorrentes.
- Redução de erros operacionais.
- Estados, validações e consequências de ações facilmente compreensíveis.
- Proteção contra exposição indevida de dados pessoais e clínicos.
- Confirmação proporcional ao risco de ações importantes.

