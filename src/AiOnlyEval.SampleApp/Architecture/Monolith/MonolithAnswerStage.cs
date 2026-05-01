namespace AiOnlyEval.SampleApp.Architecture.Monolith;

public sealed class MonolithAnswerStage
{
    public string Compose(PolicyExecutionContext context, IReadOnlyList<string> sources)
    {
        string answer = PolicyKnowledgeBase.ComposeAnswer(context.Scenario);

        context.AddTrace(
            "MonolithAnswerStage",
            "answer.compose",
            "retrieved context + user input",
            answer,
            sources,
            MonolithPolicyAssistantPipeline.Metadata("MonolithAiPipeline"));

        return answer;
    }
}
