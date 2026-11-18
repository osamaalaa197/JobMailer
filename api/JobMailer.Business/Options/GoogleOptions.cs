namespace JobMailer.Business.Options;

public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Must exactly match an authorized redirect URI in Google Cloud Console.</summary>
    public string RedirectUri { get; set; } = "http://localhost:5080/api/auth/google/callback";

    public string[] Scopes { get; set; } =
    {
        "openid",
        "email",
        "profile",
        "https://www.googleapis.com/auth/gmail.send"
    };
}
