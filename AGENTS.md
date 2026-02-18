# NeuralNetwork Project Context

## Technical Stack
- **Language:** C# (.NET 9.0 or later).
- **Environment:** Linux (Ubuntu) via WSL2.
- **Serialization:** System.Text.Json (Strict naming policy: CamelCase).

## Core Concepts
1. **The Network (Phenotype):** A Feed-Forward Neural Network.
   - **Config File:** `NetworkSetup.json`
   - **Key Properties:** `InputCount`, `OutputCount`, `Layers` (List of int), `ActivationFunction`.
   
2. **The Genetic Algorithm (Evolutionary Engine):** - **Config File:** `GASetup.json`
   - **Goal:** Optimize `NetworkSetup.json` parameters to minimize error on the test data set.
   - **Key Properties:** `PopulationSize`, `MutationRate`, `ElitismCount`, `TargetError`.

## Workflow Rules
- **Paths:** Always use Linux-style paths (e.g., `./Data/iris.csv`). Do not use Windows backslashes.
- **Verification:** When modifying the GA logic, ensure that `Crossover` and `Mutation` methods never produce an invalid `NetworkSetup` (e.g., 0 neurons in a layer).
- **Logging:** All training runs should log their "Best Fitness" to a local CSV file for analysis.