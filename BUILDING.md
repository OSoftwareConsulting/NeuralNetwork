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

In this environment, default parallel MSBuild execution may fail without surfaced errors.
Use the provided wrapper that forces single-node mode (`-m:1`):

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
- If `dotnet restore` or `dotnet build` fails with no explicit error lines, use:
  - `./scripts/dotnet-safe.sh <command> ...`
