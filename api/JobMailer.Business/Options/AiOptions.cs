namespace JobMailer.Business.Options;

public sealed class AiOptions
{
    public const string SectionName = "Ai";
    public bool Enabled { get; set; } = true;

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-1.5-flash";

    public int TimeoutSeconds { get; set; } = 45;
}
