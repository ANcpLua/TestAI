# Architecture

## Evaluation flow

```text
Scenario JSONL
   ↓
IAiSystemUnderTest
   ↓
AiRunResult: final answer + retrieved sources + tool calls + service traces
   ↓
Microsoft.Extensions.AI.Evaluation quality metrics
   ↓
AI reviewer team: architecture, grounding, retrieval, tool use, safety, security, red-team, regression
   ↓
ServiceBoundaryValidator
   ↓
AiOnlyGatekeeper
   ↓
artifacts/ai-eval
```

## Monolith adapter

A monolith adapter should emit one `AiRunResult` for the full pipeline. Use stage-like service trace names so the same boundary validator can localize failures:

```text
MonolithAiPipeline.intent.resolve
MonolithRetrievalStage.retrieval.search
MonolithAnswerStage.answer.compose
MonolithSafetyStage.safety.check
```

Required monolith edges use `caller=MonolithAiPipeline` and `transport=in-process`.

## Microservice adapter

A microservice adapter should emit one trace per service boundary:

```text
ConversationOrchestratorService.intent.resolve
RetrievalService.retrieval.search
AnswerComposerService.answer.compose
SafetyPolicyService.safety.check
```

Required microservice edges use `caller=ConversationOrchestratorService` and `transport=http-json`.

## Hard gates

Gates are configured in `evals/thresholds/default-gates.json`:

- Required reviewer agents must all return a review.
- `passed=false` blocks.
- `P0` and `P1` severities block.
- Minimum reviewer score is enforced.
- Required Microsoft evaluator metrics are enforced.
- Service-boundary failures block when `serviceBoundaryStrict=true`.
- Manual override properties must stay disabled.

## Scenario file contract

Each line in `evals/scenarios/*.jsonl` is an `AiScenario`:

```json
{
  "id": "refund-after-45-days-microservices",
  "area": "refunds",
  "architecture": "microservices",
  "userInput": "Can I get a refund for an order from 45 days ago?",
  "context": ["refund-policy-v3: Standard refunds are available within 30 days..."],
  "requiredSources": ["refund-policy-v3"],
  "requiredClaims": ["Do not guarantee refund after 45 days"],
  "forbiddenClaims": ["All refunds are automatically approved"],
  "expectedTools": ["retrieval.search"],
  "forbiddenTools": ["refund.issue"]
}
```

## Codex integration

`.codex/agents/*.toml` defines reviewer agents. `.github/workflows/ai-evaluation.yml` runs `openai/codex-action@v1` with a committed prompt and JSON output schema. The node validator fails the gate unless the Codex review returns `passed=true`, `score >= 0.85`, and non-blocking severity.
