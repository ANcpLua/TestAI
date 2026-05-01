# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project

MSTest-based AI evaluation test suite targeting .NET 10. Tests exercise Azure OpenAI chat completions via `Microsoft.Extensions.AI`, evaluate response quality using `Microsoft.Extensions.AI.Evaluation.Quality`, evaluate content safety using the prerelease `Microsoft.Extensions.AI.Evaluation.Safety` package, and persist/cache evaluation results with `Microsoft.Extensions.AI.Evaluation.Reporting`.

Authentication uses `Azure.Identity` (`DefaultAzureCredential`). `AZURE_OPENAI_ENDPOINT` is required in user secrets or environment variables. `AZURE_TENANT_ID` is optional. `AZURE_OPENAI_DEPLOYMENT` is optional and defaults to `gpt-5`.

Foundry safety tests additionally require `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`, and `AZURE_AI_PROJECT` for a hub-based Azure AI Foundry project that supports the Evaluation service.

## Build & Test

```bash
dotnet build TestAI.slnx
dotnet test TestAI.slnx                    # run all tests
dotnet test TestAI.slnx --filter GeneralAnswerQualityAndPromptContract
dotnet tool restore
dotnet tool run aieval report --path .artifacts/evaluation --output .artifacts/evaluation/report.html
```

Tests require Azure OpenAI access. Safety tests also require Azure AI Foundry Evaluation service access. Missing endpoint/auth/Foundry config should make live evaluation tests inconclusive with a clear message, not fail during initialization. Evaluation artifacts are written under `.artifacts/evaluation/`.

## Architecture

Single-project solution (`TestAI/`). `MyTests.cs` owns the live evaluation harness, creates disk-backed `ScenarioRun` instances, samples responses through the scenario chat configuration so response caching applies, and validates LLM-as-judge quality metrics, Foundry-backed safety metrics, and small local prompt-contract metrics. `MSTestSettings.cs` enables method-level parallelism for the assembly, but live evaluation tests are marked `[DoNotParallelize]`.
