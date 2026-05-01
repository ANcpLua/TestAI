using AiOnlyEval.Core.Boundaries;
using AiOnlyEval.Core.Evaluation;
using AiOnlyEval.Core.Infrastructure;
using AiOnlyEval.Core.Models;
using AiOnlyEval.Core.Reporting;
using AiOnlyEval.Core.Reviewers;
using AiOnlyEval.Core.Scenarios;
using AiOnlyEval.SampleApp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiOnlyEval.EvaluationTests;

[TestClass]
[DoNotParallelize]
public sealed class AiOnlyEvaluationTests
{
    [TestMethod]
    [DataRow("evals/scenarios/refund-policy.jsonl")]
    [DataRow("evals/scenarios/security.jsonl")]
    public async Task ScenarioPackPassesAiOnlyEvaluationGates(string scenarioPack)
    {
        if (!AiClientFactory.HasOpenAiEnvironment())
        {
            Assert.Inconclusive("Set OPENAI_API_KEY and AI_EVAL_REVIEW_MODEL to run live AI-only evaluation gates.");
        }

        string root = TestPaths.FindRepositoryRoot();
        string artifactRoot = Environment.GetEnvironmentVariable("AI_EVAL_ARTIFACT_DIR")
            ?? Path.Combine(root, "artifacts", "ai-eval");

        IReadOnlyList<AiScenario> scenarios = ScenarioLoader.LoadJsonl(Path.Combine(root, scenarioPack));
        GatePolicy policy = GatePolicy.Load(Path.Combine(root, "evals", "thresholds", "default-gates.json"));
        IReadOnlyList<ServiceBoundaryContract> serviceContracts = ServiceBoundaryContract.LoadMany(Path.Combine(root, "evals", "service-boundaries", "refund-services.json"));

        IChatClient judgeClient = AiClientFactory.CreateOpenAiChatClientFromEnvironment();
        ChatConfiguration chatConfiguration = new(judgeClient);

        var systemUnderTest = new PolicyAssistantSystemUnderTest();
        var qualityEvaluator = new MicrosoftQualityEvaluator(chatConfiguration);
        var reviewerTeam = AiReviewerTeam.CreateDefault(judgeClient);
        var writer = new EvaluationArtifactWriter(artifactRoot);

        var failures = new List<string>();
        var gates = new List<GateResult>();

        foreach (AiScenario scenario in scenarios)
        {
            AiRunResult run = await systemUnderTest.RunAsync(scenario, CancellationToken.None);
            IReadOnlyList<MetricScore> scores = await qualityEvaluator.EvaluateAsync(scenario, run, CancellationToken.None);
            IReadOnlyList<AgentReview> reviews = await reviewerTeam.ReviewAsync(scenario, run, scores, CancellationToken.None);
            IReadOnlyList<string> boundaryFailures = ServiceBoundaryValidator.Validate(scenario, run, serviceContracts);

            GateResult gate = AiOnlyGatekeeper.Evaluate(scenario, run, scores, reviews, boundaryFailures, policy);
            gates.Add(gate);
            await writer.WriteScenarioAsync(scenario, run, scores, reviews, boundaryFailures, gate, CancellationToken.None);

            if (!gate.Passed)
            {
                failures.Add(gate.ToFailureMessage());
            }
        }

        await writer.WriteSummaryAsync(gates, CancellationToken.None);
        await writer.WriteJUnitAsync(gates, CancellationToken.None);

        Assert.IsEmpty(failures, string.Join("\n", failures));
    }
}
