using System.Net;
using System.Net.Mail;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Exceptions;
using JobMailer.Business.Models;
using JobMailer.Business.Options;
using JobMailer.Data.Entities;
using JobMailer.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Profile = JobMailer.Data.Entities.Profile;

namespace JobMailer.Business.Services;

public sealed class EmailService(
    IUserRepository users,
    IProfileRepository profiles,
    IApplicationRepository applications,
    ITokenProtector protector,
    IOptions<GoogleOptions> googleOptions,
    IOptions<TrackingOptions> trackingOptions,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly GoogleOptions _google = googleOptions.Value;
    private readonly TrackingOptions _tracking = trackingOptions.Value;

    public async Task<ApplicationDto> SendAsync(
        int userId,
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.To) || !MailAddress.TryCreate(request.To, out _))
        {
            throw new AppException("A valid recipient email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new AppException("The email subject is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new AppException("The email body is required.");
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException("Your session is no longer valid.");

        var profile = await profiles.GetByIdAsync(request.ProfileId, userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        var accessToken = await GetAccessTokenAsync(user, cancellationToken);

        var trackingToken = Guid.NewGuid().ToString("N");
        using var message = BuildMessage(user, profile, request, BuildTrackingUrl(trackingToken));
        using var stream = new MemoryStream();
        message.WriteTo(stream);

        var raw = Convert.ToBase64String(stream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var credential = GoogleCredential.FromAccessToken(accessToken);
        using var gmail = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "JobMailer"
        });

        try
        {
            await gmail.Users.Messages
                .Send(new Message { Raw = raw }, "me")
                .ExecuteAsync(cancellationToken);
        }
        catch (GoogleApiException ex)
        {
            var reason = ex.Error?.Message ?? ex.Message;
            logger.LogWarning(ex, "Gmail send failed for user {UserId}.", userId);
            throw new AppException("Gmail rejected the message: " + reason, 502);
        }

        var application = new Application
        {
            UserId = user.UserId,
            ProfileId = profile.Id,
            ProfileTitle = profile.Title,
            Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim(),
            JobTitle = string.IsNullOrWhiteSpace(request.JobTitle) ? profile.Title : request.JobTitle.Trim(),
            ToEmail = request.To.Trim(),
            Subject = request.Subject.Trim(),
            SentAt = DateTime.UtcNow,
            TrackingToken = trackingToken
        };

        await applications.AddAsync(application, cancellationToken);
        await applications.SaveChangesAsync(cancellationToken);

        return new ApplicationDto(
            application.Id,
            application.Company,
            application.JobTitle,
            application.ProfileTitle,
            application.ToEmail,
            application.Subject,
            application.SentAt,
            application.OpenCount > 0,
            application.OpenCount,
            application.FirstOpenedAt,
            application.LastOpenedAt);
    }

    private async Task<string> GetAccessTokenAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = protector.Unprotect(user.AccessToken);
        if (!string.IsNullOrWhiteSpace(accessToken)
            && user.TokenExpiry is { } expiry
            && expiry > DateTime.UtcNow.AddMinutes(1))
        {
            return accessToken;
        }

        var refreshToken = protector.Unprotect(user.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAppException("Gmail access has expired. Please sign in with Google again.");
        }

        TokenResponse refreshed;
        try
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _google.ClientId,
                    ClientSecret = _google.ClientSecret
                },
                Scopes = _google.Scopes
            });

            refreshed = await flow.RefreshTokenAsync(user.GoogleId, refreshToken, cancellationToken);
        }
        catch (TokenResponseException ex)
        {
            logger.LogWarning(ex, "Google token refresh failed for user {UserId}.", user.UserId);
            throw new UnauthorizedAppException("Gmail access has expired. Please sign in with Google again.");
        }

        user.AccessToken = protector.Protect(refreshed.AccessToken);
        user.TokenExpiry = DateTime.UtcNow.AddSeconds(refreshed.ExpiresInSeconds ?? 3600);

        if (!string.IsNullOrWhiteSpace(refreshed.RefreshToken))
        {
            user.RefreshToken = protector.Protect(refreshed.RefreshToken);
        }

        await users.SaveChangesAsync(cancellationToken);
        return refreshed.AccessToken;
    }

    private static MimeMessage BuildMessage(
        User user,
        Profile profile,
        SendEmailRequest request,
        string trackingUrl)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(user.FullName, user.Email));

        if (MailAddress.TryCreate(profile.Email, out _))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(profile.Email));
        }

        message.To.Add(MailboxAddress.Parse(request.To.Trim()));
        message.Subject = request.Subject.Trim();

        var body = new BodyBuilder
        {
            TextBody = request.Body,
            HtmlBody = BuildHtmlBody(request.Body, trackingUrl)
        };

        if (!string.IsNullOrWhiteSpace(profile.CvBase64))
        {
            var fileName = string.IsNullOrWhiteSpace(profile.CvFileName) ? "CV.pdf" : profile.CvFileName;
            var bytes = Convert.FromBase64String(profile.CvBase64);
            var mimeType = GetMimeType(fileName);

            body.Attachments.Add(fileName, bytes, ContentType.Parse(mimeType));
        }

        message.Body = body.ToMessageBody();
        return message;
    }

    private string BuildTrackingUrl(string trackingToken)
    {
        var baseUrl = _tracking.PublicBaseUrl.Trim().TrimEnd('/');
        return $"{baseUrl}/api/tracking/open/{Uri.EscapeDataString(trackingToken)}";
    }

    private static string BuildHtmlBody(string text, string trackingUrl)
    {
        var encoded = WebUtility.HtmlEncode(text)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace("\n", "<br />");

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, Helvetica, sans-serif; font-size: 14px; line-height: 1.5; color: #222;">
            <div>{encoded}</div>
            <img src="{WebUtility.HtmlEncode(trackingUrl)}" width="1" height="1" alt="" style="display:block; width:1px; height:1px; border:0; opacity:0;" />
            </body>
            </html>
            """;
    }

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
}
