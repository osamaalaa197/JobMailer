using System.Text;
using System.Text.Json;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Models;
using JobMailer.Business.Options;
using Microsoft.Extensions.Options;

namespace JobMailer.Business.Services;

public sealed class OpenAiCompatibleEmailEnhancer(
    IHttpClientFactory httpClientFactory,
    IOptions<AiOptions> options) : IAiEmailEnhancer
{
    private readonly AiOptions _options = options.Value;

    public async Task<RenderedEmail> EnhanceAsync(
        string subject,
        string body,
        string jobDescription,
        string? company,
        string? jobTitle,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new RenderedEmail(subject, body);
        }

        var client = httpClientFactory.CreateClient("ai");
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, _options.TimeoutSeconds));

        var systemPrompt = """
            You are an expert career coach and professional email writer.
            Rewrite the supplied job application email so it is more compelling and tailored to the job description.
            Keep all factual candidate details unchanged (name, phone, email, skills, experience).
            Do not invent new facts, degrees, employers, or metrics.
            Incorporate relevant keywords from the job description naturally.
            Keep the subject concise and professional.
            Keep the body between 120 and 220 words.
            Return ONLY valid JSON in this exact shape: {"subject":"...","body":"..."}.
            """;

        var userPrompt = $"""
            Job description:
            {jobDescription}

            Company: {company ?? "not specified"}
            Job title: {jobTitle ?? "not specified"}

            Current subject:
            {subject}

            Current body:
            {body}

            Return only JSON.
            """;

        var payload = new
        {
            model = _options.Model,
            temperature = 0.4,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
        };

        var endpoint = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), "chat/completions");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AI request failed ({(int)response.StatusCode}): {raw}");
        }

        using var document = JsonDocument.Parse(raw);
        var content = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return ParseEnhancedEmail(content, subject, body);
    }

    private static RenderedEmail ParseEnhancedEmail(string content, string fallbackSubject, string fallbackBody)
    {
        try
        {
            using var json = JsonDocument.Parse(ExtractJson(content));
            var subject = json.RootElement.TryGetProperty("subject", out var subjectElement)
                ? subjectElement.GetString()
                : null;
            var body = json.RootElement.TryGetProperty("body", out var bodyElement)
                ? bodyElement.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                return new RenderedEmail(fallbackSubject, fallbackBody);
            }

            return new RenderedEmail(subject.Trim(), body.Trim());
        }
        catch (JsonException)
        {
            return new RenderedEmail(fallbackSubject, fallbackBody);
        }
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed[firstBrace..(lastBrace + 1)];
            }
        }

        return trimmed;
    }
}
