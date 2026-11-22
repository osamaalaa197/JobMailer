namespace JobMailer.Business.Options;

public sealed class TrackingOptions
{
    public const string SectionName = "Tracking";

    /// <summary>
    /// Public origin used in tracking URLs, for example https://jobmailer.runasp.net. When empty,
    /// the configured Web:Url is used.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}
