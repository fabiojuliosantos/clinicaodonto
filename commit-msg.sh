#!/usr/bin/env bash
#
# commit-msg.sh — Gera uma mensagem de commit (Conventional Commits) para as
# mudanças staged, usando Ollama + .junie/guidelines.md +
# .junie/skills/commit/SKILL.md. Pede confirmação e executa o git commit.
#
# Uso:
#   git add <arquivos>
#   ./commit-msg.sh
#
# Requer: git, ollama (com o modelo já baixado, ex: qwen2.5-coder:14b)

set -euo pipefail

MODEL="${OLLAMA_MODEL:-qwen2.5-coder:14b}"
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
GUIDELINES="$REPO_ROOT/.junie/guidelines.md"
SKILL="$REPO_ROOT/.junie/skills/commit/SKILL.md"

if [[ ! -f "$GUIDELINES" ]]; then
  echo "Erro: não encontrei $GUIDELINES" >&2
  exit 1
fi

if [[ ! -f "$SKILL" ]]; then
  echo "Erro: não encontrei $SKILL" >&2
  exit 1
fi

DIFF="$(git -C "$REPO_ROOT" diff --staged)"

if [[ -z "$DIFF" ]]; then
  echo "Nada staged. Rode 'git add <arquivos>' antes de gerar a mensagem."
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
  echo "# Instruções de Geração de Mensagem de Commit"
  echo
  cat "$SKILL"
  echo
  echo "---"
  echo
  echo "# Diff staged a analisar"
  echo
  echo '```diff'
  echo "$DIFF"
  echo '```'
} > "$PROMPT_FILE"

echo "Gerando mensagem de commit com $MODEL..."
echo

MSG="$(ollama run "$MODEL" < "$PROMPT_FILE")"

echo
echo "Mensagem sugerida:"
echo "──────────────────────────────────────"
echo "$MSG"
echo "──────────────────────────────────────"
echo

read -rp "Deseja commitar com essa mensagem? [s/N] " CONFIRM

if [[ "${CONFIRM,,}" == "s" ]]; then
  git -C "$REPO_ROOT" commit -m "$MSG"
  echo
  echo "Commit realizado. Rode 'git push' quando estiver pronto."
else
  echo "Commit cancelado. Mensagem copiada acima para uso manual."
fi
