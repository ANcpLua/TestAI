using AiOnlyEval.Core.Models;

namespace AiOnlyEval.Core.Reviewers;

public interface IReviewerAgent
{
    string Name { get; }

    Task<AgentReview> ReviewAsync(
        AiScenario scenario,
        AiRunResult runResult,
        IReadOnlyList<MetricScore> evaluatorScores,
        CancellationToken cancellationToken = default);
}
