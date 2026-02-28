#!/usr/bin/env bash
set -euo pipefail

SDK_VERSION="$(dotnet --version)"
SDK_BASE_DIR="$(dotnet --info | awk -F': ' '/Base Path/ {print $2}' | tr -d '\r')"

if [[ -z "${SDK_BASE_DIR:-}" ]]; then
  if [[ -d "/usr/share/dotnet/sdk/$SDK_VERSION" ]]; then
    SDK_BASE_DIR="/usr/share/dotnet/sdk/$SDK_VERSION/"
  elif [[ -d "/usr/lib/dotnet/sdk/$SDK_VERSION" ]]; then
    SDK_BASE_DIR="/usr/lib/dotnet/sdk/$SDK_VERSION/"
  else
    echo "Unable to locate SDK base directory for version $SDK_VERSION."
    exit 1
  fi
fi

SDK_ROOT="${SDK_BASE_DIR%/}/Sdks"

sudo mkdir -p \
  "$SDK_ROOT/Microsoft.NET.SDK.WorkloadAutoImportPropsLocator/Sdk" \
  "$SDK_ROOT/Microsoft.NET.SDK.WorkloadManifestTargetsLocator/Sdk"

printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadAutoImportPropsLocator/Sdk/Sdk.props" >/dev/null
printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadAutoImportPropsLocator/Sdk/AutoImport.props" >/dev/null
printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadAutoImportPropsLocator/Sdk/Sdk.targets" >/dev/null

printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadManifestTargetsLocator/Sdk/Sdk.props" >/dev/null
printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadManifestTargetsLocator/Sdk/WorkloadManifest.targets" >/dev/null
printf '<Project/>\n' | sudo tee "$SDK_ROOT/Microsoft.NET.SDK.WorkloadManifestTargetsLocator/Sdk/Sdk.targets" >/dev/null

echo "Workload locator SDK stubs created under: $SDK_ROOT"
