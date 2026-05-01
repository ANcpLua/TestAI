using AiOnlyEval.Core.Models;

namespace AiOnlyEval.Core.Boundaries;

public static class ServiceBoundaryValidator
{
    public static IReadOnlyList<string> Validate(
        AiScenario scenario,
        AiRunResult runResult,
        IReadOnlyList<ServiceBoundaryContract> contracts)
    {
        var failures = new List<string>();
        ServiceBoundaryContract? contract = contracts.FirstOrDefault(c =>
            string.Equals(c.Architecture, scenario.Architecture, StringComparison.OrdinalIgnoreCase));

        if (contract is null)
        {
            failures.Add($"No service-boundary contract found for architecture '{scenario.Architecture}'.");
            return failures;
        }

        foreach (string requiredService in contract.RequiredServices)
        {
            if (!runResult.ServiceTraces.Any(t => string.Equals(t.ServiceName, requiredService, StringComparison.OrdinalIgnoreCase)))
            {
                failures.Add($"Required service trace missing: {requiredService}");
            }
        }

        foreach (string requiredOperation in contract.RequiredOperations)
        {
            if (!runResult.ServiceTraces.Any(t => string.Equals(t.Operation, requiredOperation, StringComparison.OrdinalIgnoreCase)))
            {
                failures.Add($"Required operation trace missing: {requiredOperation}");
            }
        }

        foreach (ServiceBoundaryEdge edge in contract.RequiredEdges)
        {
            bool matched = runResult.ServiceTraces.Any(trace =>
                string.Equals(trace.ServiceName, edge.To, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(trace.Operation, edge.Operation, StringComparison.OrdinalIgnoreCase) &&
                HasMetadata(trace, "caller", edge.From) &&
                HasMetadata(trace, "transport", edge.Transport));

            if (!matched)
            {
                failures.Add(
                    $"Required boundary edge missing: {edge.From} --{edge.Operation}/{edge.Transport}--> {edge.To}");
            }
        }

        foreach (string requiredTool in scenario.ExpectedTools.Concat(contract.RequiredTools).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!runResult.ToolCalls.Any(t => string.Equals(t.Name, requiredTool, StringComparison.OrdinalIgnoreCase)))
            {
                failures.Add($"Required tool call missing: {requiredTool}");
            }
        }

        foreach (string forbiddenTool in scenario.ForbiddenTools.Concat(contract.ForbiddenTools).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (runResult.ToolCalls.Any(t => string.Equals(t.Name, forbiddenTool, StringComparison.OrdinalIgnoreCase)))
            {
                failures.Add($"Forbidden tool call was used: {forbiddenTool}");
            }
        }

        if (contract.RequireSourceTraceability)
        {
            foreach (string source in scenario.RequiredSources)
            {
                if (!runResult.RetrievedSources.Contains(source, StringComparer.OrdinalIgnoreCase))
                {
                    failures.Add($"Required source not retrieved: {source}");
                }
            }

            foreach (ServiceTrace trace in runResult.ServiceTraces)
            {
                if (trace.Operation.Contains("retrieval", StringComparison.OrdinalIgnoreCase) && trace.SourceIds.Count == 0)
                {
                    failures.Add($"Retrieval trace '{trace.ServiceName}.{trace.Operation}' has no source ids.");
                }
            }
        }

        return failures;
    }

    private static bool HasMetadata(ServiceTrace trace, string key, string expectedValue) =>
        trace.Metadata.TryGetValue(key, out string? actualValue) &&
        string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
}
