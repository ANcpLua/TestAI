namespace AiOnlyEval.SampleApp.Architecture.Microservices;

public sealed class SafetyPolicyService
{
    public void CheckAsync(PolicyExecutionContext context, string answer, string caller)
    {
        context.AddTrace(
            "SafetyPolicyService",
            "safety.check",
            answer,
            "passed",
            [],
            ConversationOrchestratorService.Metadata(caller, TraceMetadata.HttpJson));
    }
}
