using System.ComponentModel.DataAnnotations;
using JobMailer.Business.Models;

namespace JobMailer.Api.Contracts;

public sealed record ProfileRequest(
    [Required, MaxLength(120)] string Title,
    [Required, MaxLength(200)] string FullName,
    [Required, MaxLength(40)] string Phone,
    [Required, EmailAddress, MaxLength(255)] string Email,
    string? CvBase64,
    string? CvFileName)
{
    public ProfileInput ToInput() => new(Title, FullName, Phone, Email, CvBase64, CvFileName);
}

public sealed record RenderRequest(
    [Range(1, int.MaxValue)] int ProfileId,
    [Range(1, int.MaxValue)] int TemplateId,
    string? Company,
    string? JobTitle,
    [MaxLength(200)] string? ReceiverName,
    [MaxLength(12000)] string? JobDescription,
    bool UseAi = false);

public sealed record SendRequest(
    [Range(1, int.MaxValue)] int ProfileId,
    [Required, EmailAddress] string To,
    [Required, MaxLength(400)] string Subject,
    [Required] string Body,
    string? Company,
    string? JobTitle);

public sealed record TemplateRequest(
    [Required, MaxLength(120)] string Key,
    [Required, MaxLength(200)] string Subject,
    [Required] string Body)
{
    public TemplateInput ToInput() => new(Key, Subject, Body);
}
