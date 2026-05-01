using AiOnlyEval.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiOnlyEval.Core.Evaluation;

public sealed class MicrosoftQualityEvaluator
{
    private readonly ChatConfiguration _chatConfiguration;
    private readonly IReadOnlyList<IEvaluator> _evaluators;

    public MicrosoftQualityEvaluator(ChatConfiguration chatConfiguration)
    {
        _chatConfiguration = chatConfiguration;
        _evaluators = new IEvaluator[]
        {
            new RelevanceEvaluator(),
            new CoherenceEvaluator(),
            new CompletenessEvaluator(),
            new GroundednessEvaluator()
        };
    }

    public async Task<IReadOnlyList<MetricScore>> EvaluateAsync(
        AiScenario scenario,
        AiRunResult runResult,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, scenario.SystemPrompt ?? "You are a helpful assistant."),
            new(ChatRole.User, BuildUserPrompt(scenario, runResult))
        };

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, runResult.FinalAnswer));
        var scores = new List<MetricScore>();

        foreach (IEvaluator evaluator in _evaluators)
        {
            EvaluationResult result = await evaluator.EvaluateAsync(
                messages,
                response,
                _chatConfiguration,
                additionalContext: null,
                cancellationToken: cancellationToken);

            foreach (EvaluationMetric metric in result.Metrics.Values)
            {
                if (metric is NumericMetric numeric)
                {
                    scores.Add(new MetricScore
                    {
                        Name = metric.Name,
                        Value = numeric.Value ?? 0.0,
                        Reason = metric.Reason,
                        Interpretation = metric.Interpretation?.ToString()
                    });
                }
            }
        }

        return scores;
    }

    private static string BuildUserPrompt(AiScenario scenario, AiRunResult runResult)
    {
        return $$"""
        User input:
        {{scenario.UserInput}}

        Expected behavior claims:
        {{string.Join("\n", scenario.RequiredClaims.Select(c => "- " + c))}}

        Forbidden claims:
        {{string.Join("\n", scenario.ForbiddenClaims.Select(c => "- " + c))}}

        Provided context:
        {{scenario.ContextBlock}}

        Retrieved context:
        {{string.Join("\n", runResult.RetrievedContext)}}

        Assistant answer:
        {{runResult.FinalAnswer}}
        """;
    }
}
