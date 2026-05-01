namespace AiOnlyEval.SampleApp.Architecture.Microservices;

public sealed class RetrievalService
{
    private readonly PolicyToolExecutor _tools = new();

    public IReadOnlyList<string> SearchAsync(PolicyExecutionContext context, string caller)
    {
        IReadOnlyList<string> sources = _tools.SearchPolicy(context, context.Scenario.UserInput);
        _tools.LookupOrderIfRequired(context);

        context.AddTrace(
            "RetrievalService",
            "retrieval.search",
            context.Scenario.UserInput,
            string.Join(", ", sources),
            sources,
            ConversationOrchestratorService.Metadata(caller, TraceMetadata.HttpJson));

        return sources;
    }
}
