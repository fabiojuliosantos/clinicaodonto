#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CORE_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
AI_ROOT="$(cd "$CORE_DIR/.." && pwd)"

OUTPUT="$AI_ROOT/tmp/prompt.md"

mkdir -p "$AI_ROOT/tmp"

cat \
    "$CORE_DIR/prompts/system.md" \
    "$CORE_DIR/prompts/engineering-philosophy.md" \
    "$AI_ROOT/project/architecture.md" \
    "$AI_ROOT/project/coding-style.md" \
    "$AI_ROOT/project/quality-standards.md" \
    "$CORE_DIR/context/review.md" \
    "$CORE_DIR/skills/review.md" \
> "$OUTPUT"

echo "$OUTPUT"