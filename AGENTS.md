**Scope:** This repository contains a .NET 10 MSTest AI evaluation harness and AI-only architecture-boundary scaffold.

<not_yet_implemented>
</not_yet_implemented>

## Live Evaluation Harness
**does:** Runs Azure OpenAI responses through `Microsoft.Extensions.AI.Evaluation.Quality`, prerelease Foundry safety evaluators, and disk-backed `ScenarioRun` reporting under `.artifacts/evaluation`.
**does_not_do:** Fail test initialization when live Azure OpenAI or Foundry auth is missing; those cases are inconclusive with a clear message.

## AI-Only Framework
**does:** Runs JSONL scenario packs through `AiOnlyEval.Core`, `AiOnlyEval.SampleApp`, and `AiOnlyEval.Runner`; validates monolith and microservice boundary contracts, tool calls, source traces, reviewer verdicts, and hard gates.
**does_not_do:** Require human/manual review gates; allow missing required service traces, required tools, or source traceability when strict gates are enabled.

## Sample Architecture
**does:** Implements `monolith` as `MonolithAiPipeline` plus in-process stages and `microservices` as `ConversationOrchestratorService` calling service components over `http-json` trace edges.
**does_not_do:** Call external production services or perform destructive/sample-forbidden tools such as `refund.issue`, `payment.capture`, or `account.delete`.

## Scaffold Generator
**does:** Regenerates the SDK facade, Codex reviewer agents, AI-only policy, service-boundary contracts, Codex review prompt/schema, and project Codex config.
**does_not_do:** Generate product-specific system-under-test adapters; implement those behind `IAiSystemUnderTest`.

## Patterns
- Build and test through `TestAI.slnx`.
- Keep central package versions in `Directory.Packages.props`.
- Use `TimeProvider.System.GetUtcNow()` for generated timestamps.
- Mark live evaluation tests `[DoNotParallelize]`.
- Represent architecture calls with `ServiceTrace.Metadata["caller"]` and `ServiceTrace.Metadata["transport"]`.
- Use `Assert.Inconclusive` for missing live credentials or unavailable live evaluation services.

## Anti-Patterns
- Building only one project when solution-level integration changed.
- Inline package versions in project files.
- `DateTimeOffset.UtcNow` or `DateTime.UtcNow` for timestamps.
- Method-parallel live model/reporting tests.
- Service-boundary validation from service names alone.
- Failing tests for missing live credentials that should be optional locally.

## Build & Test
- `dotnet build TestAI.slnx`
- `dotnet test TestAI.slnx`
- `dotnet test TestAI.slnx --filter GeneralAnswerQualityAndPromptContract`
- `dotnet tool restore`
- `dotnet tool run aieval report --path .artifacts/evaluation --output .artifacts/evaluation/report.html`
- `dotnet run --project eng/AiOnlyEval.ScaffoldGenerator/AiOnlyEval.ScaffoldGenerator.csproj`
