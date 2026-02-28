# Building NeuralNetwork

## Prerequisites

- .NET SDK `9.0.x`
- `GeneticAlgorithmLib` repository checked out at:
  - `../GeneticAlgorithm/GeneticAlgorithmLib/GeneticAlgorithmLib.csproj`

## First-time environment fix (if needed)

If restore fails with `MSB4276` mentioning missing workload locator SDK directories, run:

```bash
./scripts/fix-dotnet-workload-locators.sh
```

This creates the missing workload locator SDK stubs under the active SDK's `Sdks` directory.

## Build commands

On this Ubuntu/.NET 9 environment, default multi-node MSBuild may fail with
`Build FAILED` and no surfaced warnings or errors.
Use the provided wrapper for build-related commands because it forces single-node mode (`-m:1`):

```bash
./scripts/dotnet-safe.sh restore NeuralNetwork.sln -v minimal
./scripts/dotnet-safe.sh build NeuralNetwork.sln -v minimal
```

## Run tests

```bash
./scripts/dotnet-safe.sh build SetupLib.Tests/SetupLib.Tests.csproj -v minimal
./scripts/dotnet-safe.sh run --project SetupLib.Tests/SetupLib.Tests.csproj
```

## Troubleshooting

- If you still see `MSB4276`, re-run:
  - `./scripts/fix-dotnet-workload-locators.sh`
- If plain `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet publish`, or `dotnet pack`
  fails with `Build FAILED` and no explicit error lines, use:
  - `./scripts/dotnet-safe.sh <command> ...`
