using System.Text.Json;
using AiOnlyEval.Core;

namespace AiOnlyEval.Core.Boundaries;

public sealed record ServiceBoundaryContract
{
    public required string Name { get; init; }
    public required string Architecture { get; init; }
    public IReadOnlyList<string> RequiredServices { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredOperations { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ServiceBoundaryEdge> RequiredEdges { get; init; } = Array.Empty<ServiceBoundaryEdge>();
    public IReadOnlyList<string> RequiredTools { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ForbiddenTools { get; init; } = Array.Empty<string>();
    public bool RequireSourceTraceability { get; init; } = true;

    public static IReadOnlyList<ServiceBoundaryContract> LoadMany(string path)
    {
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<IReadOnlyList<ServiceBoundaryContract>>(json, JsonOptions.Default)
               ?? throw new InvalidOperationException($"Unable to load service-boundary contracts: {path}");
    }
}

public sealed record ServiceBoundaryEdge
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Operation { get; init; }
    public required string Transport { get; init; }
}
