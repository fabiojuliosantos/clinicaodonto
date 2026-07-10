#!/usr/bin/env bash

set -euo pipefail

COMMAND="${1:-}"

case "$COMMAND" in
  review)
    cat <<EOF
core/prompts/system.md
core/prompts/engineering-philosophy.md
project/project-glossary.md
project/architecture.md
project/architecture-decisions.md
project/stack.md
project/coding-style.md
project/quality-standards.md
project/workflow.md
project/domain.md
EOF
    ;;
  *)
    echo "Unknown context command: $COMMAND" >&2
    exit 1
    ;;
esac