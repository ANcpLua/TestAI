namespace AiOnlyEval.SampleApp.Architecture.Monolith;

public sealed class MonolithRetrievalStage
{
    private readonly PolicyToolExecutor _tools = new();

    public IReadOnlyList<string> Retrieve(PolicyExecutionContext context)
    {
        IReadOnlyList<string> sources = _tools.SearchPolicy(context, context.Scenario.UserInput);
        _tools.LookupOrderIfRequired(context);

        context.AddTrace(
            "MonolithRetrievalStage",
            "retrieval.search",
            context.Scenario.UserInput,
            string.Join(", ", sources),
            sources,
            MonolithPolicyAssistantPipeline.Metadata("MonolithAiPipeline"));

        return sources;
    }
}
