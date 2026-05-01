namespace AiOnlyEval.SampleApp.Architecture.Monolith;

public sealed class MonolithSafetyStage
{
    public void Check(PolicyExecutionContext context, string answer)
    {
        context.AddTrace(
            "MonolithSafetyStage",
            "safety.check",
            answer,
            "passed",
            [],
            MonolithPolicyAssistantPipeline.Metadata("MonolithAiPipeline"));
    }
}
