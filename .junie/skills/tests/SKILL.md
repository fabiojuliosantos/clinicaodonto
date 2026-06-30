# Generate Tests — Odonto.Backend

Gere **testes novos** para o código alterado (diff atual), seguindo os
padrões de teste definidos em `.junie/guidelines.md`. Não modifique o código
de produção — apenas proponha os testes.

## Padrões obrigatórios

- **Framework**: xUnit.
- **Asserções**: FluentAssertions (`resultado.Should().Be(...)`, nunca
  `Assert.Equal`).
- **Mocks**: NSubstitute (`Substitute.For<IRepositorio>()`, nunca Moq).
- **Nomenclatura**: `Metodo_Cenario_ResultadoEsperado`, em português. Ex.:
  `Confirmar_QuandoJaCancelado_DeveLancarExcecao`,
  `CriarPaciente_ComCpfInvalido_DeveRetornarErroDeValidacao`.
- **Estrutura**: Arrange / Act / Assert, com comentários `// Arrange`,
  `// Act`, `// Assert` separando os blocos.
- **Projeto de destino**:
  - Regras de domínio (entidades, value objects, agregados) →
    `Odonto.Domain.Tests`.
  - Casos de uso, Services, validações →  `Odonto.Application.Tests`.
  - Indique claramente em qual projeto cada classe de teste deve ficar.

## O que priorizar ao gerar os testes

1. **Caminho feliz**: o comportamento esperado funcionando corretamente.
2. **Regras de negócio do domínio**: principalmente transições de estado
   (ex.: `SituacaoAgendamento`), que têm regras explícitas no
   `guidelines.md` — cubra também as transições **inválidas** que devem
   lançar exceção.
3. **Validações** (FluentValidation): campos obrigatórios, formatos
   inválidos (ex.: Cpf malformado), limites de valor.
4. **Edge cases**: coleções vazias, valores nulos, datas limítrofes.

Não gere testes triviais demais (ex.: testar getter/setter sem lógica) nem
testes de infraestrutura (EF Core, banco) — isso é fora do escopo deste
skill, que foca em Domain e Application.

## Se o código alterado não tiver lógica testável

Diga isso explicitamente (ex.: "este arquivo é só um DTO sem lógica, não há
o que testar aqui") em vez de forçar testes artificiais.

## Formato da resposta

Para cada classe de teste sugerida:

1. Caminho do arquivo (ex.: `Odonto.Domain.Tests/AgendamentoTests.cs`).
2. Código completo da classe de teste, pronto para colar.
3. Uma linha breve explicando o que aquele conjunto de testes cobre, caso
   não seja óbvio pelo nome dos métodos.
