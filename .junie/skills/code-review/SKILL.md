# Code Review — Odonto.Backend

Revise o código selecionado, o arquivo aberto, ou o diff atual (commits não
enviados / staged), cruzando com as guidelines do projeto definidas em
`.junie/guidelines.md`. **Este skill é somente leitura: aponte problemas e
sugestões, não aplique alterações de código automaticamente**, salvo pedido
explícito do desenvolvedor.

## Checklist de revisão

### 1. Arquitetura (Clean Architecture)
- Domain não pode referenciar EF Core, ASP.NET ou qualquer framework externo.
- Application só depende de Domain (nunca de Infrastructure ou API).
- Infrastructure implementa interfaces definidas em Domain/Application —
  nunca o contrário.
- Repositórios seguem o padrão "um por agregado", com interface no Domain.
- Nenhuma regra de negócio vazando para Controllers ou Services — regras de
  domínio (ex.: transições de `SituacaoAgendamento`) devem viver na entidade.

### 2. Stack e padrões do projeto
- Controllers, não Minimal API.
- Sem CQRS/MediatR — sinalizar se aparecer Handler/Command/Query desnecessário.
- Validação deve estar em FluentValidation na camada Application, não
  espalhada em ifs dentro de Controllers ou Services.
- Mapeamento manual — sinalizar se for introduzido AutoMapper ou similar.

### 3. Linguagem ubíqua e nomenclatura
- Domínio em português (entidades, value objects, enums, métodos de regra de
  negócio); termos técnicos/plataforma em inglês (`Repository`, `Service`,
  `Validator`, `Controller`, `Dto`, `Id`, `CreatedAt`, `UpdatedAt`).
- Verbos de CRUD em português: `Criar`, `Atualizar`, `Remover`, `Obter`,
  `Listar`. Verbos de regra de negócio em português: `Confirmar`, `Cancelar`,
  `Remarcar`, `Realizar`.
- Sem acento em identificadores de código (acento só em strings de usuário).
- Mesmo termo do domínio com o mesmo nome em todas as camadas — sinalizar
  qualquer tradução parcial ou inconsistente (ex.: `Status` misturado com
  `Situacao` para o mesmo conceito).

### 4. Regras de negócio específicas do domínio
- `Agendamento` e `Consulta` são conceitos distintos — uma `Consulta` só deve
  ser criada a partir de `Agendamento.Realizar()`.
- Transições de `SituacaoAgendamento` devem respeitar o ciclo de vida válido
  (ex.: não permitir `Confirmar()` em um agendamento já `Cancelado`).
- Módulo de estoque (`Insumo`, `MovimentacaoEstoque`) ainda está em
  confirmação de modelagem — se o código implementar algo além do que está
  descrito nas guidelines, sinalizar para confirmar com o desenvolvedor antes
  de seguir.
- Pagamento, convênio e plano de saúde são fora de escopo — sinalizar
  qualquer código que comece a modelar essas áreas.

### 5. Bugs e qualidade geral
- Edge cases não tratados (nulos, coleções vazias, datas inválidas).
- Performance: queries N+1, loops desnecessários, carregamento desnecessário
  de coleções via EF Core (`Include` faltando ou excessivo).
- Segurança: validação de entrada, possível SQL injection (se houver SQL cru
  fora do EF Core), exposição de dados sensíveis em DTOs/respostas.
- Legibilidade: nomes pouco claros, métodos longos demais, duplicação óbvia.

### 6. Testes
- Verificar se a mudança revisada tem testes correspondentes.
- Testes devem seguir `Metodo_Cenario_ResultadoEsperado`, em português,
  estrutura Arrange/Act/Assert, usando xUnit + FluentAssertions + NSubstitute.
- Projeto de teste correto: `Odonto.Domain.Tests` para regras de domínio,
  `Odonto.Application.Tests` para casos de uso.

## Formato da resposta

Liste os problemas encontrados em ordem de severidade (**Crítico** →
**Importante** → **Sugestão**), e para cada um:

1. Onde está (arquivo/linha ou trecho).
2. Por que é um problema (referência à guideline ou regra violada).
3. Sugestão de correção (sem aplicar automaticamente).

Se não houver problemas relevantes, diga isso explicitamente em vez de forçar
apontamentos triviais.
