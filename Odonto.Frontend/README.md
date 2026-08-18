# Odonto Frontend

Aplicação interna da Almeida Estética e Sorriso, construída com Vue 3,
TypeScript e Vite.

O diretório `.proto/` contém a referência visual e permanece separado do código
da aplicação.

## Requisitos

- Node.js 22.12 ou superior
- npm

## Comandos

```bash
npm install
npm run dev
npm run type-check
npm run test:run
npm run build
npm run test:e2e
```

## Organização

```text
src/
├── app/                 # Inicialização e roteamento
├── assets/styles/       # Tokens e estilos globais
├── modules/             # Funcionalidades organizadas por domínio
└── shared/              # Código reutilizável sem domínio específico
    ├── api/
    ├── components/
    ├── composables/
    ├── types/
    └── utils/
```

Os módulos `auth`, `dashboard` e `team` são apenas diretórios reservados nesta
etapa. Nenhuma tela foi implementada.

Copie `.env.example` para `.env.local` e ajuste `VITE_API_BASE_URL` conforme a
URL local da API. Nunca versione credenciais ou segredos em arquivos `.env`.
