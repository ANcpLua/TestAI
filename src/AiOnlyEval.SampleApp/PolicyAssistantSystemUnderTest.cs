using AiOnlyEval.Core.Models;
using AiOnlyEval.SampleApp.Architecture;

namespace AiOnlyEval.SampleApp;

public sealed class PolicyAssistantSystemUnderTest : IAiSystemUnderTest
{
    public Task<AiRunResult> RunAsync(AiScenario scenario, CancellationToken cancellationToken = default)
    {
        IPolicyAssistantArchitecture architecture = PolicyAssistantArchitectureFactory.Create(scenario.Architecture);
        return architecture.RunAsync(scenario, cancellationToken);
    }
}
