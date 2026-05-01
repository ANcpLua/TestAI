using AiOnlyEval.Core.Models;

namespace AiOnlyEval.SampleApp;

public interface IAiSystemUnderTest
{
    Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default);
}
