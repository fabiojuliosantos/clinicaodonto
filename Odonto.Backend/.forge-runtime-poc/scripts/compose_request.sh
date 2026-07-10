#!/usr/bin/env bash

set -euo pipefail

COMMAND="${1:-review}"
INPUT_MODE="${2:-diff}"

AI_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT="$AI_ROOT/tmp/request.txt"

mkdir -p "$AI_ROOT/tmp"

{
    echo "========================"
    echo "SYSTEM"
    echo "========================"
    cat "$AI_ROOT/core/prompts/system.md"

    echo
    echo "========================"
    echo "ENGINEERING PHILOSOPHY"
    echo "========================"
    cat "$AI_ROOT/core/prompts/engineering-philosophy.md"

    echo
    echo "========================"
    echo "PROJECT CONTEXT"
    echo "========================"

    while IFS= read -r file; do
        echo
        echo "----- $file -----"
        cat "$AI_ROOT/$file"
    done < <("$AI_ROOT/scripts/resolvers/context.sh" "$COMMAND")

    echo
    echo "========================"
    echo "TASK"
    echo "========================"
    "$AI_ROOT/scripts/resolvers/task.sh" "$COMMAND"

    echo
    echo "========================"
    echo "INPUT"
    echo "========================"
    "$AI_ROOT/scripts/resolvers/input.sh" "$INPUT_MODE"

} > "$OUTPUT"

echo "$OUTPUT"