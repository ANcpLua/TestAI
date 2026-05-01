using AiOnlyEval.Core.Models;
using AiOnlyEval.Core.Scenarios;
using AiOnlyEval.SampleApp;
using AiOnlyEval.SampleApp.Architecture;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiOnlyEval.EvaluationTests;

[TestClass]
public sealed class ArchitectureTopologyTests
{
    [TestMethod]
    public async Task MicroserviceScenarioEmitsHttpJsonServiceEdges()
    {
        AiScenario scenario = LoadScenario("refund-after-45-days-microservices");
        var system = new PolicyAssistantSystemUnderTest();

        AiRunResult run = await system.RunAsync(scenario, CancellationToken.None);

        AssertMetadata(run, TraceMetadata.Architecture, ArchitectureNames.Microservices);
        AssertEdge(run, "ConversationOrchestratorService", "RetrievalService", "retrieval.search", TraceMetadata.HttpJson, TraceMetadata.Microservice);
        AssertEdge(run, "ConversationOrchestratorService", "AnswerComposerService", "answer.compose", TraceMetadata.HttpJson, TraceMetadata.Microservice);
        AssertEdge(run, "ConversationOrchestratorService", "SafetyPolicyService", "safety.check", TraceMetadata.HttpJson, TraceMetadata.Microservice);
    }

    [TestMethod]
    public async Task MonolithScenarioEmitsInProcessStageEdges()
    {
        AiScenario scenario = LoadScenario("refund-check-order-monolith");
        var system = new PolicyAssistantSystemUnderTest();

        AiRunResult run = await system.RunAsync(scenario, CancellationToken.None);

        AssertMetadata(run, TraceMetadata.Architecture, ArchitectureNames.Monolith);
        AssertEdge(run, "MonolithAiPipeline", "MonolithRetrievalStage", "retrieval.search", TraceMetadata.InProcess, TraceMetadata.MonolithStage);
        AssertEdge(run, "MonolithAiPipeline", "MonolithAnswerStage", "answer.compose", TraceMetadata.InProcess, TraceMetadata.MonolithStage);
        AssertEdge(run, "MonolithAiPipeline", "MonolithSafetyStage", "safety.check", TraceMetadata.InProcess, TraceMetadata.MonolithStage);
    }

    private static AiScenario LoadScenario(string scenarioId)
    {
        string root = TestPaths.FindRepositoryRoot();
        return Directory.GetFiles(Path.Combine(root, "evals", "scenarios"), "*.jsonl")
            .SelectMany(ScenarioLoader.LoadJsonl)
            .FirstOrDefault(s => string.Equals(s.Id, scenarioId, StringComparison.Ordinal))
            ?? throw new AssertFailedException($"Scenario '{scenarioId}' was not found.");
    }

    private static void AssertEdge(
        AiRunResult run,
        string caller,
        string serviceName,
        string operation,
        string transport,
        string boundaryKind)
    {
        ServiceTrace trace = run.ServiceTraces.FirstOrDefault(t =>
            string.Equals(t.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(t.Operation, operation, StringComparison.OrdinalIgnoreCase))
            ?? throw new AssertFailedException($"Expected trace {caller} --{operation}/{transport}--> {serviceName}.");

        AssertMetadata(trace, TraceMetadata.Caller, caller);
        AssertMetadata(trace, TraceMetadata.Transport, transport);
        AssertMetadata(trace, TraceMetadata.BoundaryKind, boundaryKind);
    }

    private static void AssertMetadata(AiRunResult run, string key, string expected)
    {
        if (!run.Metadata.TryGetValue(key, out string? actual))
        {
            throw new AssertFailedException($"Run metadata missing '{key}'.");
        }

        Assert.AreEqual(expected, actual);
    }

    private static void AssertMetadata(ServiceTrace trace, string key, string expected)
    {
        if (!trace.Metadata.TryGetValue(key, out string? actual))
        {
            throw new AssertFailedException($"Trace metadata missing '{key}' on {trace.ServiceName}.{trace.Operation}.");
        }

        Assert.AreEqual(expected, actual);
    }
}
