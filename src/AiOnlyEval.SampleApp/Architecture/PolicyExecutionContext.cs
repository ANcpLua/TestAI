using AiOnlyEval.Core.Models;

namespace AiOnlyEval.SampleApp.Architecture;

public sealed class PolicyExecutionContext
{
    private readonly List<ToolCallTrace> _toolCalls = [];
    private readonly List<ServiceTrace> _serviceTraces = [];

    public PolicyExecutionContext(AiScenario scenario)
    {
        Scenario = scenario;
        RetrievedSources =
            scenario.RequiredSources.Count > 0
                ? [.. scenario.RequiredSources]
                : ["refund-policy-v3"];
        RetrievedContext = [.. scenario.Context];
    }

    public AiScenario Scenario { get; }

    public IReadOnlyList<string> RetrievedSources { get; }

    public IReadOnlyList<string> RetrievedContext { get; }

    public IReadOnlyList<ToolCallTrace> ToolCalls => _toolCalls;

    public IReadOnlyList<ServiceTrace> ServiceTraces => _serviceTraces;

    public void AddToolCall(string name, Dictionary<string, string> arguments, string resultSummary)
    {
        _toolCalls.Add(
            new ToolCallTrace
            {
                Name = name,
                Arguments = arguments,
                ResultSummary = resultSummary
            });
    }

    public void AddTrace(
        string serviceName,
        string operation,
        string inputSummary,
        string outputSummary,
        IReadOnlyList<string>? sourceIds,
        Dictionary<string, string> metadata)
    {
        _serviceTraces.Add(
            new ServiceTrace
            {
                ServiceName = serviceName,
                Operation = operation,
                InputSummary = inputSummary,
                OutputSummary = outputSummary,
                SourceIds = sourceIds ?? [],
                Metadata = metadata
            });
    }

    public AiRunResult ToRunResult(string finalAnswer, string architecture)
    {
        return new AiRunResult
        {
            ScenarioId = Scenario.Id,
            FinalAnswer = finalAnswer,
            RetrievedSources = RetrievedSources,
            RetrievedContext = RetrievedContext,
            ToolCalls = ToolCalls,
            ServiceTraces = ServiceTraces,
            Metadata =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["sample"] = "true",
                    ["architecture"] = architecture
                }
        };
    }
}
