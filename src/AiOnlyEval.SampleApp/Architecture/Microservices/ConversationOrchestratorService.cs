using AiOnlyEval.Core.Models;
using AiOnlyEval.SampleApp.Architecture;

namespace AiOnlyEval.SampleApp.Architecture.Microservices;

public sealed class ConversationOrchestratorService : IPolicyAssistantArchitecture
{
    private readonly RetrievalService _retrieval = new();
    private readonly AnswerComposerService _answer = new();
    private readonly SafetyPolicyService _safety = new();

    public string Architecture => ArchitectureNames.Microservices;

    public Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default)
    {
        var context = new PolicyExecutionContext(scenario);

        context.AddTrace(
            "ConversationOrchestratorService",
            "intent.resolve",
            scenario.UserInput,
            "refund-policy-question",
            [],
            Metadata("external-client", TraceMetadata.InProcess));

        IReadOnlyList<string> sources = _retrieval.SearchAsync(context, "ConversationOrchestratorService");
        string answer = _answer.ComposeAsync(context, sources, "ConversationOrchestratorService");
        _safety.CheckAsync(context, answer, "ConversationOrchestratorService");

        return Task.FromResult(context.ToRunResult(answer, Architecture));
    }

    internal static Dictionary<string, string> Metadata(string caller, string transport) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [TraceMetadata.Architecture] = ArchitectureNames.Microservices,
            [TraceMetadata.BoundaryKind] = TraceMetadata.Microservice,
            [TraceMetadata.Caller] = caller,
            [TraceMetadata.Transport] = transport,
            [TraceMetadata.ComponentKind] = "service"
        };
}
