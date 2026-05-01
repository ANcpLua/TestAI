# AI-Only .NET Evaluation Framework

This repository is a concrete .NET 10 framework for automated evaluation of AI systems with **0 manual review gates**.

It combines:

- `Microsoft.Extensions.AI.Evaluation` quality evaluators.
- Scenario files in JSONL.
- A panel of AI reviewer agents that review the system under test.
- Hard gates for safety, grounding, tool use, service-boundary behavior, and regression checks.
- CI automation with `dotnet test`, an executable evaluation runner, and Codex review automation.
- Reporting hooks for local artifacts, JUnit XML, and the Microsoft `dotnet aieval` report tool.
- A Meziantou-style scaffold generator under `eng/AiOnlyEval.ScaffoldGenerator`.
- A sample architecture adapter that exercises true monolith and microservice trace shapes.

## What this scaffold tests

```text
user input
  -> input/service-boundary checks
  -> retrieval/context selection
  -> prompt/system under test
  -> model/tool/service trace
  -> Microsoft quality evaluators
  -> AI reviewer-agent panel
  -> hard gate
  -> JSON/Markdown/JUnit/HTML artifacts
```

## Requirements

- .NET 10 SDK or later.
- `OPENAI_API_KEY` for evaluator/reviewer agents.
- `AI_EVAL_REVIEW_MODEL`, for example `gpt-4.1` or your chosen review model.
- Optional: Azure AI Foundry credentials if you later enable `Microsoft.Extensions.AI.Evaluation.Safety` evaluators.

## Run locally

Linux/macOS:

```bash
export OPENAI_API_KEY="..."
export AI_EVAL_REVIEW_MODEL="gpt-4.1"
./scripts/run-evals.sh
```

Windows PowerShell:

```powershell
$env:OPENAI_API_KEY="..."
$env:AI_EVAL_REVIEW_MODEL="gpt-4.1"
./scripts/run-evals.ps1
```

Direct runner:

```bash
dotnet run --project src/AiOnlyEval.Runner/AiOnlyEval.Runner.csproj -- \
  --scenario evals/scenarios/refund-policy.jsonl \
  --scenario evals/scenarios/security.jsonl \
  --out artifacts/ai-eval
```

## Main outputs

```text
artifacts/ai-eval/runs/*.json
artifacts/ai-eval/runs/*.md
artifacts/ai-eval/summary.json
artifacts/ai-eval/junit-ai-eval.xml
artifacts/ai-eval/index.html
artifacts/test-results/*.trx
artifacts/codex/codex-ai-review.json
```

## AI-only policy

The gate policy is encoded in:

```text
evals/thresholds/default-gates.json
```

The default policy has:

```json
{
  "aiOnlyPolicy": {
    "humanReviewRequired": false,
    "manualOverrideAllowed": false,
    "manualApprovalSteps": 0
  }
}
```

The build accepts automated evaluator verdicts, AI reviewer-agent verdicts, service-boundary validators, and CI checks only.

## Architecture-specific use

The included sample app has two real execution shapes selected by `AiScenario.Architecture`:

- `monolith`: `MonolithAiPipeline` runs retrieval, answer composition, and safety as in-process stages.
- `microservices`: `ConversationOrchestratorService` calls retrieval, answer composition, and safety service components over `http-json` trace edges.

### Monolith

Point `IAiSystemUnderTest` to the monolith's real AI endpoint or internal pipeline. Run the scenario suite against the full monolith flow. Keep stage traces such as `MonolithRetrievalStage`, `MonolithAnswerStage`, and `MonolithSafetyStage` with `transport=in-process` so failures remain localizable.

### Microservices

Keep each service contract in:

```text
evals/service-boundaries/*.json
```

The validator checks that traces emitted by the system under test include required service calls, operations, caller-to-service edges, transport metadata, source IDs, and tool decisions.

## Repository map

```text
src/AiOnlyEval.Core/                 Evaluation framework, gates, agents, artifact writer
src/AiOnlyEval.SampleApp/            Example system under test
src/AiOnlyEval.Runner/               CLI runner for CI and local execution
src/Sdk/                             Generated SDK facade/enforcement files
eng/AiOnlyEval.ScaffoldGenerator/    Regenerates policy, SDK, Codex agents, and CI scaffolding
tests/AiOnlyEval.EvaluationTests/    CI-ready MSTest evaluation suite
evals/scenarios/                     JSONL scenario packs
evals/thresholds/                    Hard-gate policy
evals/service-boundaries/            Microservice/monolith boundary contracts
.codex/agents/                       Project-scoped Codex reviewer agents
.github/workflows/                   CI pipeline
.github/codex/                       Codex review prompt and JSON schema
scripts/                             Local and CI runner helpers
```

## Replace the sample app

Replace `PolicyAssistantSystemUnderTest` with an adapter to your real system:

```csharp
public sealed class ProductionSystemUnderTest : IAiSystemUnderTest
{
    public async Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default)
    {
        // Call your monolith endpoint, orchestrator service, or local app pipeline.
        // Capture service traces, retrieved sources, tool calls, and final answer.
    }
}
```

The evaluator layer does not care whether the system under test is a monolith or microservices. It only needs an `AiRunResult`.

## Regenerate scaffold files

```bash
dotnet run --project eng/AiOnlyEval.ScaffoldGenerator/AiOnlyEval.ScaffoldGenerator.csproj
```
