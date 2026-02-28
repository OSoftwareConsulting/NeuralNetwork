#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 1 ]; then
  echo "Usage: $0 <dotnet-command> [args...]" >&2
  echo "Example: $0 build NeuralNetwork.sln -v minimal" >&2
  exit 2
fi

subcommand="$1"
shift

# Force single-node MSBuild execution for build-related commands.
# On this Linux/.NET SDK setup, default multi-node MSBuild can fail with
# "Build FAILED" and no surfaced errors, while -m:1 succeeds reliably.
case "$subcommand" in
  restore|build|test|publish|pack)
    exec dotnet "$subcommand" -m:1 "$@"
    ;;
  *)
    exec dotnet "$subcommand" "$@"
    ;;
esac
