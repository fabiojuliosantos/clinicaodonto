#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CORE_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

PROMPT="$("$CORE_DIR/scripts/builders/assemble_context.sh")"

echo "Prompt generated:"
echo "$PROMPT"