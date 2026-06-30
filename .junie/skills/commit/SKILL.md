# Commit Message — Odonto.Backend

Gere **apenas a mensagem de commit** para as mudanças staged (`git diff
--staged`). **Não execute o commit** — devolva a mensagem para o
desenvolvedor revisar e rodar manualmente.

## Regras de formato (Conventional Commits)

```
<tipo>: <descrição em português, no imperativo, minúscula, sem ponto final>
```

### Tipos válidos (em inglês)

- `feat` — nova funcionalidade
- `fix` — correção de bug
- `test` — adição ou ajuste de testes
- `chore` — tarefas de manutenção, configuração, dependências
- `refactor` — mudança de código sem alterar comportamento externo
- `docs` — documentação

### Descrição (em português)

- Imperativo: "adiciona", "corrige", "remove" — nunca "adicionado",
  "adicionando" ou "added".
- Minúscula no início, sem ponto final.
- Objetiva: o que mudou e, se relevante, por quê — não como.
- Usar a linguagem ubíqua do domínio quando aplicável (ex.: "agendamento",
  "consulta", "insumo", não traduções literais como "appointment",
  "schedule").

### Exemplos corretos

```
feat: adiciona confirmação de agendamento
fix: corrige cálculo de duração do procedimento
test: adiciona testes do agregado Agendamento
refactor: extrai validação de Cpf para value object
chore: atualiza pacote do EF Core
docs: documenta regra de ciclo de vida do agendamento
```

## Se o diff misturar mais de um tipo de mudança

Sinalize isso explicitamente e sugira separar em commits menores, propondo
uma mensagem para cada parte lógica, em vez de forçar um único `tipo` genérico
para tudo.

## Se o diff envolver código fora do escopo do projeto

Se identificar que o diff começa a modelar pagamento, convênio ou plano de
saúde (fora de escopo definido em `.junie/guidelines.md`), avise antes de
sugerir a mensagem — pode ser um sinal de que o código não deveria estar
sendo commitado ainda.

## Formato da resposta

Devolva **somente a mensagem de commit pronta**, em um bloco de código, sem
explicações adicionais — a menos que haja um alerta relevante (diff misto ou
fora de escopo), que deve vir antes do bloco.
