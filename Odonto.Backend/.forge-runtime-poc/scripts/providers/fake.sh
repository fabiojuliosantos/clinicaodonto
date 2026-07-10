#!/usr/bin/env bash

set -euo pipefail

REQUEST_FILE="${1:-}"

echo "Forge Provider Fake"
echo
echo "Request received:"
echo "$REQUEST_FILE"
echo
echo "Preview:"
echo "------------------------"
head -n 40 "$REQUEST_FILE"
