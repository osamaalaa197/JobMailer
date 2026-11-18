using System.ComponentModel.DataAnnotations;
using JobMailer.Business.Models;

namespace JobMailer.Api.Contracts;

public sealed record ProfileRequest(
    [property: Required, MaxLength(120)] string Title,
    [property: Required, MaxLength(200)] string FullName,
    [property: Required, MaxLength(40)] string Phone,
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    string? CvBase64,
    string? CvFileName)
{
    public ProfileInput ToInput() => new(Title, FullName, Phone, Email, CvBase64, CvFileName);
}

public sealed record RenderRequest(
    [property: Range(1, int.MaxValue)] int ProfileId,
    [property: Range(1, int.MaxValue)] int TemplateId,
    string? Company,
    string? JobTitle);

public sealed record SendRequest(
    [property: Range(1, int.MaxValue)] int ProfileId,
    [property: Required, EmailAddress] string To,
    [property: Required, MaxLength(400)] string Subject,
    [property: Required] string Body,
    string? Company,
    string? JobTitle);
