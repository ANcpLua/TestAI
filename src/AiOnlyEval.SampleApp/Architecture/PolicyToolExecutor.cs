using AiOnlyEval.Core.Models;

namespace AiOnlyEval.SampleApp.Architecture;

public sealed class PolicyToolExecutor
{
    public IReadOnlyList<string> SearchPolicy(PolicyExecutionContext context, string query)
    {
        IReadOnlyList<string> sources = PolicyKnowledgeBase.RetrieveSources(context.Scenario);
        context.AddToolCall(
            "retrieval.search",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["query"] = query,
                ["area"] = context.Scenario.Area
            },
            $"Retrieved {string.Join(", ", sources)}");

        return sources;
    }

    public void LookupOrderIfRequired(PolicyExecutionContext context)
    {
        if (!context.Scenario.ExpectedTools.Contains("order.lookup", StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        context.AddToolCall(
            "order.lookup",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["orderReference"] = "scenario-provided-or-redacted"
            },
            "Order metadata looked up without issuing a refund.");
    }
}
