using AiOnlyEval.Core.Models;

namespace AiOnlyEval.SampleApp.Architecture;

public static class PolicyAssistantArchitectureFactory
{
    public static IPolicyAssistantArchitecture Create(string architecture) =>
        architecture.Trim().ToLowerInvariant() switch
        {
            ArchitectureNames.Monolith => new Monolith.MonolithPolicyAssistantPipeline(),
            ArchitectureNames.Microservices => new Microservices.ConversationOrchestratorService(),
            _ => throw new InvalidOperationException($"Unsupported architecture '{architecture}'.")
        };
}

public interface IPolicyAssistantArchitecture
{
    string Architecture { get; }

    Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default);
}

public static class ArchitectureNames
{
    public const string Monolith = "monolith";
    public const string Microservices = "microservices";
}
