#!/usr/bin/env bash

set -euo pipefail

MODE="${1:-diff}"

case "$MODE" in
  diff)
    git --no-pager diff HEAD
    ;;

  staged)
    git --no-pager diff --staged
    ;;

  *)
    if [[ -f "$MODE" ]]; then
      cat "$MODE"
    elif [[ -d "$MODE" ]]; then
      find "$MODE" -type f \
        \( -name "*.cs" -o -name "*.csproj" -o -name "*.json" -o -name "*.md" \) \
        -not -path "*/bin/*" \
        -not -path "*/obj/*" \
        -print0 |
      while IFS= read -r -d '' file; do
        echo
        echo "===== FILE: $file ====="
        cat "$file"
      done
    else
      echo "Unknown input source: $MODE" >&2
      exit 1
    fi
    ;;
esac