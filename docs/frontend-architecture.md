# Arquitetura do frontend

## Stack adotada

- Vue 3
- TypeScript em modo estrito
- Vite
- Composition API com `<script setup>`
- Vue Router para navegação
- Pinia somente para estado realmente global
- Vitest para testes unitários e de componentes
- Playwright para fluxos críticos no navegador

O frontend será uma aplicação independente que consumirá a API ASP.NET Core por
HTTP/JSON. Autenticação e autorização serão integradas ao mecanismo de JWT do
backend.

## Diretrizes

- Componentes devem ter responsabilidades pequenas e nomes orientados ao
  domínio da clínica.
- Estado local deve permanecer no componente ou fluxo que o utiliza. Não mover
  dados para a Pinia sem necessidade compartilhada real.
- Acesso HTTP deve ficar encapsulado, sem requisições espalhadas pelos
  componentes visuais.
- Tipos de requisição e resposta devem ser explícitos.
- Regras de negócio pertencem ao backend. O frontend pode orientar e validar a
  interação, mas não deve ser a única fonte de uma regra crítica.
- Estados de carregamento, vazio, sucesso, validação e erro devem ser tratados.
- Componentes e padrões existentes devem ser reutilizados antes da criação de
  variantes novas.
- Dependências novas exigem justificativa e discussão prévia.

## Referência visual

O diretório `Odonto.Frontend/.proto/` contém a identidade e os fluxos visuais de
referência. A direção atual usa DM Sans, tons suaves de azul, superfícies de
baixo contraste e uma linguagem visual denominada "Soft Calm".

Ao converter o protótipo:

- Preserve a identidade, hierarquia e intenção das interações.
- Extraia tokens e componentes reutilizáveis em vez de copiar CSS por página.
- Preserve os recursos de marca existentes.
- Mantenha responsividade, navegação por teclado, foco visível, semântica e
  suporte a `prefers-reduced-motion`.
- Adapte comportamentos demonstrativos às regras confirmadas do produto.

## Integração e segurança

- A interface pode ocultar ações não autorizadas para melhorar a experiência,
  mas a API é responsável por aplicar a autorização real.
- Não registre tokens, senhas, documentos ou informações clínicas em logs.
- Não armazene segredos no repositório ou no código entregue ao navegador.
- Tratamento de sessão e armazenamento do token serão definidos junto com o
  contrato de autenticação do backend.

## Estrutura inicial

```text
Odonto.Frontend/
├── .proto/              # Referência visual, fora da aplicação
├── src/
│   ├── app/             # Inicialização e roteamento
│   ├── assets/styles/   # Tokens e estilos globais
│   ├── modules/         # Funcionalidades organizadas por domínio
│   └── shared/          # API, componentes, composables, tipos e utilitários
└── tests/e2e/           # Fluxos críticos no navegador
```

Os primeiros módulos reservados são `auth`, `dashboard` e `team`. Subpastas
internas devem surgir conforme casos reais, evitando camadas vazias ou abstrações
antecipadas.
