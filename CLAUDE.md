# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

MSTest-based AI evaluation test suite targeting .NET 10. Tests exercise Azure OpenAI chat completions via `Microsoft.Extensions.AI` and evaluate response quality using `Microsoft.Extensions.AI.Evaluation.Quality` evaluators (coherence, relevance, etc.).

Authentication uses `Azure.Identity` (`DefaultAzureCredential`) with endpoint and tenant configured via user secrets.

## Build & Test

```bash
dotnet build TestAI.slnx
dotnet test TestAI.slnx                    # run all tests
dotnet test TestAI.slnx --filter TestCoherence  # single test by name
```

Tests require Azure OpenAI credentials in user secrets (`AZURE_OPENAI_ENDPOINT`, `AZURE_TENANT_ID`). Tests will fail without valid Azure auth.

## Architecture

Single-project solution (`TestAI/`). `MyTests.cs` owns the test lifecycle: `[ClassInitialize]` fetches a chat response once, individual `[TestMethod]`s run evaluators against that cached response. `MSTestSettings.cs` enables method-level parallelism.
