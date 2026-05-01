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

var options = CliOptions.Parse(args);
string root = options.Root ?? FindRepositoryRoot(Directory.GetCurrentDirectory());
string outputRoot = options.Output ?? Path.Combine(root, "artifacts", "ai-eval");

string[] scenarioPacks = options.Scenarios.Count > 0
    ? options.Scenarios.ToArray()
    : new[]
    {
        Path.Combine(root, "evals", "scenarios", "refund-policy.jsonl"),
        Path.Combine(root, "evals", "scenarios", "security.jsonl")
    };

GatePolicy policy = GatePolicy.Load(options.Policy ?? Path.Combine(root, "evals", "thresholds", "default-gates.json"));
IReadOnlyList<ServiceBoundaryContract> contracts = ServiceBoundaryContract.LoadMany(
    options.Boundaries ?? Path.Combine(root, "evals", "service-boundaries", "refund-services.json"));

IChatClient judgeClient = AiClientFactory.CreateOpenAiChatClientFromEnvironment();
ChatConfiguration chatConfiguration = new(judgeClient);

var systemUnderTest = new PolicyAssistantSystemUnderTest();
var qualityEvaluator = new MicrosoftQualityEvaluator(chatConfiguration);
var reviewerTeam = AiReviewerTeam.CreateDefault(judgeClient);
var writer = new EvaluationArtifactWriter(outputRoot);

var failures = new List<string>();
var allGates = new List<GateResult>();

foreach (string scenarioPack in scenarioPacks)
{
    string path = Path.IsPathRooted(scenarioPack) ? scenarioPack : Path.Combine(root, scenarioPack);
    IReadOnlyList<AiScenario> scenarios = ScenarioLoader.LoadJsonl(path);

    foreach (AiScenario scenario in scenarios)
    {
        AiRunResult run = await systemUnderTest.RunAsync(scenario, CancellationToken.None);
        IReadOnlyList<MetricScore> scores = await qualityEvaluator.EvaluateAsync(scenario, run, CancellationToken.None);
        IReadOnlyList<AgentReview> reviews = await reviewerTeam.ReviewAsync(scenario, run, scores, CancellationToken.None);
        IReadOnlyList<string> boundaryFailures = ServiceBoundaryValidator.Validate(scenario, run, contracts);
        GateResult gate = AiOnlyGatekeeper.Evaluate(scenario, run, scores, reviews, boundaryFailures, policy);

        await writer.WriteScenarioAsync(scenario, run, scores, reviews, boundaryFailures, gate, CancellationToken.None);
        allGates.Add(gate);

        Console.WriteLine($"{scenario.Id}: {(gate.Passed ? "PASS" : "FAIL")}");
        if (!gate.Passed)
        {
            string failure = gate.ToFailureMessage();
            failures.Add(failure);
            Console.Error.WriteLine(failure);
        }
    }
}

await writer.WriteSummaryAsync(allGates, CancellationToken.None);
await writer.WriteJUnitAsync(allGates, CancellationToken.None);

return failures.Count == 0 ? 0 : 1;

static string FindRepositoryRoot(string start)
{
    string dir = start;
    while (!string.IsNullOrWhiteSpace(dir))
    {
        if (Directory.Exists(Path.Combine(dir, "evals")) && Directory.Exists(Path.Combine(dir, "src")))
        {
            return dir;
        }

        DirectoryInfo? parent = Directory.GetParent(dir);
        if (parent is null) break;
        dir = parent.FullName;
    }

    throw new InvalidOperationException("Could not locate repository root. Expected directories: evals and src.");
}

internal sealed record CliOptions(string? Root, string? Output, string? Policy, string? Boundaries, List<string> Scenarios)
{
    public static CliOptions Parse(string[] args)
    {
        string? root = null;
        string? output = null;
        string? policy = null;
        string? boundaries = null;
        var scenarios = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {arg}");

            switch (arg)
            {
                case "--root": root = Next(); break;
                case "--out": output = Next(); break;
                case "--policy": policy = Next(); break;
                case "--boundaries": boundaries = Next(); break;
                case "--scenario": scenarios.Add(Next()); break;
                case "--help":
                case "-h":
                    Console.WriteLine("Usage: dotnet run --project src/AiOnlyEval.Runner -- [--scenario file.jsonl] [--out artifacts/ai-eval]");
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        return new CliOptions(root, output, policy, boundaries, scenarios);
    }
}
