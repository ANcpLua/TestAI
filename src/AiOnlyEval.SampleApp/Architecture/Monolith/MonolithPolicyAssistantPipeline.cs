using AiOnlyEval.Core.Models;
using AiOnlyEval.SampleApp.Architecture;

namespace AiOnlyEval.SampleApp.Architecture.Monolith;

public sealed class MonolithPolicyAssistantPipeline : IPolicyAssistantArchitecture
{
    private readonly MonolithRetrievalStage _retrieval = new();
    private readonly MonolithAnswerStage _answer = new();
    private readonly MonolithSafetyStage _safety = new();

    public string Architecture => ArchitectureNames.Monolith;

    public Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default)
    {
        var context = new PolicyExecutionContext(scenario);

        context.AddTrace(
            "MonolithAiPipeline",
            "intent.resolve",
            scenario.UserInput,
            "refund-policy-question",
            [],
            Metadata("MonolithAiPipeline"));

        IReadOnlyList<string> sources = _retrieval.Retrieve(context);
        string answer = _answer.Compose(context, sources);
        _safety.Check(context, answer);

        return Task.FromResult(context.ToRunResult(answer, Architecture));
    }

    internal static Dictionary<string, string> Metadata(string caller) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [TraceMetadata.Architecture] = ArchitectureNames.Monolith,
            [TraceMetadata.BoundaryKind] = TraceMetadata.MonolithStage,
            [TraceMetadata.Caller] = caller,
            [TraceMetadata.Transport] = TraceMetadata.InProcess,
            [TraceMetadata.ComponentKind] = "stage"
        };
}
