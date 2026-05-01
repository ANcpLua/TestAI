namespace AiOnlyEval.SampleApp.Architecture.Microservices;

public sealed class AnswerComposerService
{
    public string ComposeAsync(PolicyExecutionContext context, IReadOnlyList<string> sources, string caller)
    {
        string answer = PolicyKnowledgeBase.ComposeAnswer(context.Scenario);

        context.AddTrace(
            "AnswerComposerService",
            "answer.compose",
            "retrieved context + user input",
            answer,
            sources,
            ConversationOrchestratorService.Metadata(caller, TraceMetadata.HttpJson));

        return answer;
    }
}
