#!/usr/bin/env bash

set -euo pipefail

AI_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

REQUEST="$("$AI_ROOT/scripts/compose_request.sh" review)"

"$AI_ROOT/scripts/provider.sh" "$REQUEST" \
  | "$AI_ROOT/scripts/formatter.sh"