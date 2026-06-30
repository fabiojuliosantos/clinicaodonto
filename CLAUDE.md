# Odonto — Sistema de Gestão de Clínica Odontológica

Sistema de **controle interno** de uma clínica odontológica. Fluxo de leitura
simples; foco atual na **gestão clínica** e no **controle de estoque de insumos**.
Pagamento, convênio e plano de saúde são features futuras — **não modelar agora**.

---

## ⚠️ Regra fundamental de trabalho

**Não desenvolva código da aplicação por conta própria.** Sem um pedido
explícito do desenvolvedor para implementar uma funcionalidade, limite-se a:
revisar código, criar testes, gerar commits, responder dúvidas e propor planos.
Quando em dúvida se deve escrever código de produção, **pergunte antes**.

---

## Arquitetura — Clean Architecture

Quatro camadas (solution `Odonto.Backend`). Regra de dependência:

```
API ──► Application ──► Domain ◄── Infrastructure
                          ▲
                  Domain não depende de ninguém
```

- **Odonto.Domain** — entidades, agregados, value objects, enums, regras de
  negócio e **interfaces de repositório**. Zero dependência de framework.
- **Odonto.Application** — casos de uso (Services), orquestração, DTOs,
  interfaces de serviços externos. Depende apenas do Domain.
- **Odonto.Infrastructure** — EF Core, implementação dos repositórios,
  integrações. Implementa as interfaces definidas em Domain/Application.
- **Odonto.API** — controllers, injeção de dependência, configuração. Entrada.

Violar a regra de dependência (ex.: Domain referenciando EF Core) é um erro de
arquitetura e deve ser apontado em review.

## Stack e padrões

- **API**: Controllers (não Minimal API).
- **Camada de aplicação**: Services simples. **Sem CQRS/MediatR** — o fluxo de
  leitura é simples e não justifica a cerimônia no momento.
- **Acesso a dados**: EF Core com migrations.
- **Repositórios**: padrão Repository por agregado (interface no Domain,
  implementação na Infrastructure).
- **Validação**: FluentValidation, na camada Application.
- **Mapeamento**: manual (sem AutoMapper).

---

## Linguagem ubíqua (domínio)

O vocabulário do código segue o vocabulário do negócio (português). Termos
canônicos do domínio:

| Termo (PT) | Significado | Observações |
|---|---|---|
| **Paciente** | quem é atendido | Nome, Cpf, DataNascimento, Telefone, Email |
| **Dentista** | profissional | Nome, Cro, Especialidade |
| **Especialidade** | área de atuação | ex.: Ortodontia, Endodontia |
| **Agendamento** | marcação de horário | Paciente, Dentista, DataHora, Situacao |
| **Consulta** | atendimento realizado | criada quando um Agendamento é realizado |
| **Procedimento** | o que é feito | Nome, Valor, Duracao |
| **Prontuario** | histórico clínico do paciente | registros das consultas |
| **PlanoTratamento** | procedimentos planejados | Paciente, Procedimentos, Situacao |
| **Insumo** | item consumível de estoque | Nome, Unidade, QuantidadeEstoque, EstoqueMinimo |
| **MovimentacaoEstoque** | entrada/saída de insumo | Insumo, Tipo, Quantidade, Data |

### Ciclo de vida do Agendamento (regra de negócio)

`Agendamento` e `Consulta` são conceitos **separados**. O atendente marca o
horário (Agendamento) e depois sinaliza o desfecho:

`SituacaoAgendamento { Agendado, Confirmado, Realizado, Remarcado, Cancelado }`

- Marcar gera um `Agendamento` em `Agendado`.
- `Confirmar()`, `Remarcar(novaDataHora)`, `Cancelar()` — transições de regra.
- Quando `Realizar()`, gera-se uma `Consulta` vinculada.

### Estoque de insumos (módulo novo — CONFIRMAR modelagem)

Objetivo: saber **o que precisa ser comprado**. Suposições a validar:
- **Insumo** com `QuantidadeEstoque` e `EstoqueMinimo`; quando
  `QuantidadeEstoque <= EstoqueMinimo`, o item entra na lista de reposição.
- **MovimentacaoEstoque** registra `Entrada` (compra) e `Saida` (consumo) com
  `TipoMovimentacao { Entrada, Saida }`.
> Confirmar com o desenvolvedor antes de implementar este módulo.

### Fora de escopo (features futuras)

Convênio / plano de saúde, pagamento e faturamento. **Não modelar agora.**

---

## Convenções de nomenclatura

- **Português** para o domínio: entidades, value objects, enums e seus valores,
  propriedades de domínio e métodos de regra de negócio
  (`Paciente`, `Agendamento.Confirmar()`, `SituacaoAgendamento.Cancelado`).
- **Inglês** para padrões técnicos / plataforma: sufixos de padrão
  (`Repository`, `Service`, `Validator`, `Controller`, `Dto`) e membros de
  infraestrutura (`Id`, `CreatedAt`, `UpdatedAt`).
- **Verbos de CRUD em português** nos casos de uso. Verbos canônicos:
  `Criar`, `Atualizar`, `Remover`, `Obter` (por id), `Listar` (coleção).
  Exemplos: `CriarPacienteService`, `ObterPacientePorIdService`,
  `ListarAgendamentosService`, `AtualizarInsumoService`, `RemoverPacienteService`.
- **Verbos de regra de negócio** (não-CRUD) em português: `Confirmar`,
  `Cancelar`, `Remarcar`, `Realizar`.
- **Sem acento** em identificadores (`Situacao`, `Prontuario`, `Procedimento`).
  Acento apenas em strings voltadas ao usuário.
- Um termo do domínio tem **um único nome canônico** em todas as camadas
  (Domain → DTO → endpoint → tabela). Nunca traduzir pela metade.

---

## Testes

- **Framework**: xUnit. **Asserções**: FluentAssertions. **Mocks**: NSubstitute.
- **Estrutura**: projetos espelho — `Odonto.Domain.Tests`,
  `Odonto.Application.Tests` (um projeto de teste por camada testável).
- **Nomenclatura**: `Metodo_Cenario_ResultadoEsperado`, em português. Ex.:
  `Confirmar_QuandoJaCancelado_DeveLancarExcecao`.
- Padrão Arrange / Act / Assert.

---

## Commits

Conventional Commits com **tipo em inglês** e **descrição em português**:

```
feat: adiciona agendamento de consulta
fix: corrige cálculo de duração do procedimento
test: adiciona testes do agregado Agendamento
chore: ...
refactor: ...
docs: ...
```

Tipos: `feat`, `fix`, `test`, `chore`, `refactor`, `docs`.

---

## Ambiente

- .NET 10. Autenticação git via **SSH** (não usar PAT/HTTPS).
- A solution fica em `Odonto.Backend/`. `obj/`, `bin/` e `.idea/` são ignorados.
