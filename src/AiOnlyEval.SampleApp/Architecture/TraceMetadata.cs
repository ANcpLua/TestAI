namespace AiOnlyEval.SampleApp.Architecture;

public static class TraceMetadata
{
    public const string Architecture = "architecture";
    public const string BoundaryKind = "boundaryKind";
    public const string Caller = "caller";
    public const string Transport = "transport";
    public const string ComponentKind = "componentKind";

    public const string InProcess = "in-process";
    public const string HttpJson = "http-json";
    public const string MonolithStage = "monolith-stage";
    public const string Microservice = "microservice";
}
