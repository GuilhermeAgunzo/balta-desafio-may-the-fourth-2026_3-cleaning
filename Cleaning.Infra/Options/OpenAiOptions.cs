namespace Cleaning.Infra.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;

    public string ModelId { get; init; } = "gpt-4o-mini-2024-07-18";
}
