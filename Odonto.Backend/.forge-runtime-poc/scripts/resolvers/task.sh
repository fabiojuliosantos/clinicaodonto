#!/usr/bin/env bash

set -euo pipefail

COMMAND="${1:-}"

case "$COMMAND" in
  review)
    cat <<'EOF'
Execute a technical code review of the supplied input.

Focus on:
- correctness
- architecture
- maintainability
- security
- performance
- consistency with the project context

Report only issues supported by evidence.
If no relevant issues are found, say so explicitly.
EOF
    ;;

  *)
    echo "Unknown task command: $COMMAND" >&2
    exit 1
    ;;
esac