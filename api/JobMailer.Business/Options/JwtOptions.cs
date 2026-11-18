namespace JobMailer.Business.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "JobMailer";

    public string Audience { get; set; } = "JobMailer.Web";

    public int ExpiryHours { get; set; } = 8;
}
