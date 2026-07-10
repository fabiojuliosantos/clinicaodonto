#!/usr/bin/env bash

set -euo pipefail

AI_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

source "$AI_ROOT/core/config.sh"

PROVIDER="${FORGE_PROVIDER:-fake}"
PROVIDER_SCRIPT="$AI_ROOT/scripts/providers/${PROVIDER}.sh"

if [[ ! -x "$PROVIDER_SCRIPT" ]]; then
  echo "Provider not found or not executable: $PROVIDER" >&2
  exit 1
fi

exec "$PROVIDER_SCRIPT" "$@"
