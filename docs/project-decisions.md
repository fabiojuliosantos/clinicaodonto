# Decisões e pendências do projeto

Este documento registra decisões duradouras e pontos que ainda exigem definição.

## Decisões confirmadas

### Produto interno

Clínica Odonto é um sistema exclusivo da Almeida Estética e Sorriso, utilizado
somente por funcionários. Não é SaaS e não oferece autoatendimento a pacientes.

### Stack do frontend

Foi adotado Vue 3 com TypeScript e Vite. A escolha considera a curva de
aprendizado de um mantenedor com experiência principal em C#, sem comprometer a
capacidade de construir uma aplicação administrativa completa.

### Protótipo

`Odonto.Frontend/.proto/` é a referência visual da V1. Todos os módulos exibidos
fazem parte do escopo, mas comportamentos demonstrativos podem ser adaptados às
regras reais do produto.

### Provisionamento de usuários

Somente o login é público. A criação e a gestão de contas de funcionários
pertencem ao módulo Equipe e dependem de permissão no frontend e no backend. A
atribuição inicial de permissões ainda precisa ser definida.

### Identidade e funcionário

`AppUser` permanece um detalhe de infraestrutura do ASP.NET Core Identity e
armazena somente informações da conta de acesso. Nome, nome de exibição,
telefone de contato e referência da foto pertencem ao agregado `Funcionario`.
Cada conta possui um vínculo obrigatório e único com um funcionário por meio de
`FuncionarioId`. Cargo e permissões permanecem conceitos distintos: permissões
são aplicadas pela autorização, enquanto cargo pertence ao domínio da equipe.

### Meu perfil

O usuário autenticado é identificado pelas claims assinadas `sub`, referente à
conta do Identity, e `funcionario_id`, referente ao agregado `Funcionario`. O
e-mail não é usado como identificador porque pode ser alterado. `GET /api/me`
retorna o perfil e `PATCH /api/me` altera somente nome de exibição e telefone.
Nome completo e e-mail são somente leitura nesse fluxo; alterações
administrativas continuam pertencendo ao módulo Equipe. A foto usa endpoints
separados em `/api/me/foto`, protegidos pelo mesmo JWT. O upload aceita JPEG,
PNG ou WebP com até 2 MB, valida o conteúdo, remove metadados e gera um WebP
quadrado de 512 pixels. O agregado armazena somente uma chave aleatória; o
arquivo fica em armazenamento local configurável por
`Storage:ProfilePhotosPath`, abstraído para permitir a adoção futura de um
serviço externo sem alterar o domínio. A leitura exige autenticação e não usa
cache público porque se trata de dado pessoal.

### Autenticação JWT

Tokens de acesso são validados por assinatura, emissor, audiência e validade. A
chave deve ter pelo menos 32 bytes e ser fornecida por configuração segura do
ambiente. Roles vêm do ASP.NET Core Identity e não são presumidas nem fixadas no
token. Contas inativas não podem iniciar novas sessões.

### Provisionamento administrativo inicial

O primeiro funcionário e sua conta são criados por um seeder controlado na
inicialização da API, não por endpoint público nem por `HasData()` do EF Core. O
seeder somente executa com `Bootstrap:Enabled=true` e quando ainda não existe
nenhuma conta. `Funcionario`, `AppUser` e role configurada são persistidos na
mesma transação. As credenciais vêm de configuração segura do ambiente; após o
primeiro acesso, o bootstrap deve ser desabilitado e a senha inicial removida.

### Recuperação de senha

A solicitação de redefinição sempre retorna uma resposta genérica, exista ou
não uma conta para o e-mail informado. O código possui seis dígitos, validade
de dez minutos e é armazenado apenas como hash. Os endpoints possuem limitação
de tentativas por endereço IP. As credenciais SMTP pertencem à configuração
segura do ambiente e nunca ao repositório.

### Sessão no frontend

A sessão JWT é mantida no `sessionStorage`. Ela sobrevive a recarregamentos na
mesma aba, mas é encerrada ao fechar a aba ou ao realizar logout. Sessões
expiradas são descartadas pela guarda de navegação. O frontend não oferece
"Lembrar de mim" enquanto não houver uma decisão específica sobre persistência
duradoura. Uma futura migração para cookies `HttpOnly` exige alteração coordenada
do contrato de autenticação no backend.

### Autonomia do agente

Solicitações bem definidas podem ser implementadas e testadas diretamente.
Decisões de produto, mudanças arquiteturais relevantes, novas dependências ou
impactos no backend devem ser discutidos antes da execução.

## Pendências conhecidas

- Definir a matriz de permissões dos cinco perfis internos.
- Definir o provisionamento administrativo inicial.
- Definir o fluxo de ativação de novos funcionários criados pelo módulo Equipe.
- Evoluir o contrato de autenticação caso a sessão passe a usar cookies `HttpOnly`.
- Detalhar os requisitos e a ordem de implementação de cada módulo da V1.
- Definir ambientes, hospedagem e processo de implantação.
