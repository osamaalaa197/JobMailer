using System.Text.RegularExpressions;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Exceptions;
using JobMailer.Business.Models;
using JobMailer.Data.Entities;
using JobMailer.Data.Repositories;

namespace JobMailer.Business.Services;

public sealed class TemplateService(
    IProfileRepository profiles,
    ITemplateRepository templates) : ITemplateService
{
    private static readonly Regex PlaceholderPattern = new(
        @"\{\{\s*(?<name>[a-zA-Z0-9_]+)\s*\}\}",
        RegexOptions.Compiled);

    public async Task<IReadOnlyList<TemplateDto>> GetTemplatesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var list = await templates.GetAllAsync(userId, cancellationToken);
        return list.Select(x => new TemplateDto(x.Id, x.Key, x.Subject, x.Body)).ToList();
    }

    public async Task<int> CreateTemplateAsync(
        int userId,
        TemplateInput input,
        CancellationToken cancellationToken = default)
    {
        Validate(input);

        if ((await templates.GetAllAsync(userId, cancellationToken))
            .Any(x => string.Equals(x.Key, input.Key.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new AppException("You already have a template with this key.");
        }

        var template = new Template
        {
            UserId = userId,
            Key = input.Key.Trim(),
            Subject = input.Subject.Trim(),
            Body = input.Body.Trim()
        };

        await templates.AddAsync(template, cancellationToken);
        await templates.SaveChangesAsync(cancellationToken);
        return template.Id;
    }

    public async Task UpdateTemplateAsync(
        int userId,
        int templateId,
        TemplateInput input,
        CancellationToken cancellationToken = default)
    {
        Validate(input);

        var template = await templates.GetByIdAsync(templateId, userId, cancellationToken)
            ?? throw new NotFoundException("Template not found.");

        if ((await templates.GetAllAsync(userId, cancellationToken)).Any(x =>
                x.Id != templateId &&
                string.Equals(x.Key, input.Key.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new AppException("You already have a template with this key.");
        }

        template.Key = input.Key.Trim();
        template.Subject = input.Subject.Trim();
        template.Body = input.Body.Trim();

        await templates.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTemplateAsync(
        int userId,
        int templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await templates.GetByIdAsync(templateId, userId, cancellationToken)
            ?? throw new NotFoundException("Template not found.");

        templates.Remove(template);
        await templates.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(TemplateInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Key))
        {
            throw new AppException("Template key is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Subject))
        {
            throw new AppException("Template subject is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Body))
        {
            throw new AppException("Template body is required.");
        }
    }

    public async Task<RenderedEmail> RenderAsync(
        int userId,
        RenderEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await profiles.GetByIdAsync(request.ProfileId, userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");
        var template = await templates.GetByIdAsync(request.TemplateId, userId, cancellationToken)
            ?? throw new NotFoundException("Template not found.");

        var values = BuildValues(
            profile,
            template,
            request.Company,
            request.JobTitle,
            request.ReceiverName);

        return new RenderedEmail(
            ReplacePlaceholders(template.Subject, values),
            ReplacePlaceholders(template.Body, values));
    }

    private static Dictionary<string, string> BuildValues(
        Profile profile,
        Template template,
        string? company,
        string? jobTitle,
        string? receiverName)
    {
        var firstName = profile.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            ?? profile.FullName.Trim();

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CompanyName"] = string.IsNullOrWhiteSpace(company) ? "your company" : company.Trim(),
            ["JobTitle"] = string.IsNullOrWhiteSpace(jobTitle) ? profile.Title : jobTitle.Trim(),
            ["ReceiverName"] = string.IsNullOrWhiteSpace(receiverName) ? "Hiring Manager" : receiverName.Trim(),
            ["FullName"] = profile.FullName,
            ["FirstName"] = firstName,
            ["ProfileTitle"] = profile.Title,
            ["Phone"] = profile.Phone,
            ["Email"] = profile.Email,
            ["TemplateName"] = template.Key
        };
    }

    private static string ReplacePlaceholders(string text, IReadOnlyDictionary<string, string> values) =>
        PlaceholderPattern.Replace(text, match =>
        {
            var name = match.Groups["name"].Value;
            return values.TryGetValue(name, out var value) ? value : match.Value;
        });
}
