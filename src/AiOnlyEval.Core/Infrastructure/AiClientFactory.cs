using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using OpenAI.Chat;

namespace AiOnlyEval.Core.Infrastructure;

public static class AiClientFactory
{
    public static IChatClient CreateOpenAiChatClientFromEnvironment()
    {
        string apiKey = RequiredEnvironment("OPENAI_API_KEY");
        string model = RequiredEnvironment("AI_EVAL_REVIEW_MODEL");
        return new ChatClient(model, apiKey).AsIChatClient();
    }

    public static ChatConfiguration CreateEvaluationChatConfigurationFromEnvironment()
    {
        return new ChatConfiguration(CreateOpenAiChatClientFromEnvironment());
    }

    public static bool HasOpenAiEnvironment() =>
        HasEnvironment("OPENAI_API_KEY") && HasEnvironment("AI_EVAL_REVIEW_MODEL");

    private static string RequiredEnvironment(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required environment variable: {name}");
        }

        return value;
    }

    private static bool HasEnvironment(string name) =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name));
}
