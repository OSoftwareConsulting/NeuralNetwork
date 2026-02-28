#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 1 ]; then
  echo "Usage: $0 <dotnet-command> [args...]" >&2
  echo "Example: $0 build NeuralNetwork.sln -v minimal" >&2
  exit 2
fi

subcommand="$1"
shift

# Force single-node MSBuild execution to avoid intermittent project-graph failures.
exec dotnet "$subcommand" -m:1 "$@"
