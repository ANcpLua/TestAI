using AiOnlyEval.Core.Boundaries;
using AiOnlyEval.Core.Models;
using AiOnlyEval.Core.Scenarios;
using AiOnlyEval.SampleApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiOnlyEval.EvaluationTests;

[TestClass]
public sealed class ServiceBoundaryContractTests
{
    [TestMethod]
    public async Task SampleSystemEmitsRequiredServiceBoundaryTraces()
    {
        string root = TestPaths.FindRepositoryRoot();
        var contracts = ServiceBoundaryContract.LoadMany(Path.Combine(root, "evals", "service-boundaries", "refund-services.json"));
        var scenarios = ScenarioLoader.LoadJsonl(Path.Combine(root, "evals", "scenarios", "refund-policy.jsonl"))
            .Concat(ScenarioLoader.LoadJsonl(Path.Combine(root, "evals", "scenarios", "security.jsonl")))
            .ToArray();

        var system = new PolicyAssistantSystemUnderTest();
        var failures = new List<string>();

        foreach (AiScenario scenario in scenarios)
        {
            AiRunResult run = await system.RunAsync(scenario, CancellationToken.None);
            failures.AddRange(ServiceBoundaryValidator.Validate(scenario, run, contracts).Select(f => $"{scenario.Id}: {f}"));
        }

        Assert.IsEmpty(failures, string.Join("\n", failures));
    }
}
