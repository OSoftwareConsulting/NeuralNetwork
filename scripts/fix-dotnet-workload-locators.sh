#!/usr/bin/env bash
set -euo pipefail

SDK_ROOT="/usr/lib/dotnet/sdk/10.0.103/Sdks"

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
