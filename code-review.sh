#!/usr/bin/env bash
#
# code-review.sh — Revisa o diff atual (staged + unstaged) usando Ollama,
# cruzando com as guidelines do projeto em .junie/guidelines.md e a skill
# .junie/skills/code-review/SKILL.md.
#
# Uso:
#   ./code-review.sh                  # revisa git diff (staged + unstaged)
#   ./code-review.sh --staged         # revisa apenas staged
#   ./code-review.sh path/Arquivo.cs  # revisa um arquivo específico inteiro
#
# Requer: git, ollama (com o modelo já baixado, ex: qwen2.5-coder:7b)

set -euo pipefail

MODEL="${OLLAMA_MODEL:-qwen2.5-coder:14b}"
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
GUIDELINES="$REPO_ROOT/.junie/guidelines.md"
SKILL="$REPO_ROOT/.junie/skills/code-review/SKILL.md"

if [[ ! -f "$GUIDELINES" ]]; then
  echo "Erro: não encontrei $GUIDELINES" >&2
  exit 1
fi

if [[ ! -f "$SKILL" ]]; then
  echo "Erro: não encontrei $SKILL" >&2
  exit 1
fi

# Monta o conteúdo a ser revisado
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
  echo "Nada para revisar (diff vazio). Use --staged ou passe um arquivo."
  exit 0
fi

# Monta o prompt completo
PROMPT_FILE="$(mktemp)"
trap 'rm -f "$PROMPT_FILE"' EXIT

{
  echo "# Guidelines do projeto"
  echo
  cat "$GUIDELINES"
  echo
  echo "---"
  echo
  echo "# Instruções de Code Review"
  echo
  cat "$SKILL"
  echo
  echo "---"
  echo
  if [[ "$MODE" == "file" ]]; then
    echo "# Arquivo a revisar: $TARGET_FILE"
  else
    echo "# Diff a revisar"
  fi
  echo
  echo '```diff'
  echo "$CONTENT"
  echo '```'
} > "$PROMPT_FILE"

echo "Rodando code review com $MODEL (pode levar alguns minutos)..."
echo

ollama run "$MODEL" < "$PROMPT_FILE"
