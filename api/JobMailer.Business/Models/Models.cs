namespace JobMailer.Business.Models;

public sealed record UserInfo(int UserId, string Email, string FullName);

public sealed record LoginResult(string AccessToken, DateTime ExpiresAt, UserInfo User);

public sealed record JwtToken(string Token, DateTime ExpiresAt);

public sealed record ProfileDto(
    int Id,
    string Title,
    string FullName,
    string Phone,
    string Email,
    string? CvFileName,
    bool HasCv);

public sealed record ProfileInput(
    string Title,
    string FullName,
    string Phone,
    string Email,
    string? CvBase64,
    string? CvFileName);

public sealed record TemplateDto(int Id, string Key, string Subject, string Body);

public sealed record TemplateInput(string Key, string Subject, string Body);

public sealed record RenderEmailRequest(
    int ProfileId,
    int TemplateId,
    string? Company,
    string? JobTitle,
    string? ReceiverName = null);

public sealed record RenderedEmail(string Subject, string Body);

public sealed record SendEmailRequest(
    int ProfileId,
    string To,
    string Subject,
    string Body,
    string? Company,
    string? JobTitle);

public sealed record ApplicationDto(
    int Id,
    string? Company,
    string? JobTitle,
    string ProfileTitle,
    string ToEmail,
    string Subject,
    DateTime SentAt,
    bool Opened,
    int OpenCount,
    DateTime? FirstOpenedAt,
    DateTime? LastOpenedAt);

public sealed record DashboardDayDto(DateTime Date, int Sent, int Opened);

public sealed record CompanyStatsDto(string Company, int Sent, int Opened);

public sealed record DashboardDto(
    int TotalSent,
    int Opened,
    int Unopened,
    int TotalOpens,
    double OpenRate,
    IReadOnlyList<DashboardDayDto> DailyActivity,
    IReadOnlyList<CompanyStatsDto> TopCompanies,
    IReadOnlyList<ApplicationDto> RecentApplications);
