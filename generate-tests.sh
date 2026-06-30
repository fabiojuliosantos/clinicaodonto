#!/usr/bin/env bash
#
# generate-tests.sh — Gera testes novos para o código alterado (diff atual),
# usando Ollama + .junie/guidelines.md + .junie/skills/tests/SKILL.md.
#
# Uso:
#   ./generate-tests.sh                  # gera testes p/ diff (staged + unstaged)
#   ./generate-tests.sh --staged         # gera testes apenas p/ staged
#   ./generate-tests.sh path/Arquivo.cs  # gera testes p/ um arquivo específico inteiro
#
# Requer: git, ollama (com o modelo já baixado, ex: qwen2.5-coder:7b)

set -euo pipefail

MODEL="${OLLAMA_MODEL:-qwen2.5-coder:14b}"
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
GUIDELINES="$REPO_ROOT/.junie/guidelines.md"
SKILL="$REPO_ROOT/.junie/skills/tests/SKILL.md"

if [[ ! -f "$GUIDELINES" ]]; then
  echo "Erro: não encontrei $GUIDELINES" >&2
  exit 1
fi

if [[ ! -f "$SKILL" ]]; then
  echo "Erro: não encontrei $SKILL" >&2
  exit 1
fi

CONTENT=""
MODE="diff"

if [[ "${1:-}" == "--staged" ]]; then
  CONTENT="$(git -C "$REPO_ROOT" diff --staged)"
elif [[ -n "${1:-}" && -f "${1:-}" ]]; then
  MODE="file"
  CONTENT="$(cat "$1")"
  TARGET_FILE="$1"
else
  CONTENT="$(git -C "$REPO_ROOT" diff HEAD)"
fi

if [[ -z "$CONTENT" ]]; then
  echo "Nada para gerar testes (diff vazio). Use --staged ou passe um arquivo."
  exit 0
fi

PROMPT_FILE="$(mktemp)"
trap 'rm -f "$PROMPT_FILE"' EXIT

{
  echo "# Guidelines do projeto"
  echo
  cat "$GUIDELINES"
  echo
  echo "---"
  echo
  echo "# Instruções de Geração de Testes"
  echo
  cat "$SKILL"
  echo
  echo "---"
  echo
  if [[ "$MODE" == "file" ]]; then
    echo "# Arquivo para gerar testes: $TARGET_FILE"
  else
    echo "# Diff a analisar"
  fi
  echo
  echo '```diff'
  echo "$CONTENT"
  echo '```'
} > "$PROMPT_FILE"

echo "Gerando testes com $MODEL (pode levar alguns minutos)..."
echo

ollama run "$MODEL" < "$PROMPT_FILE"
