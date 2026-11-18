namespace JobMailer.Data.Entities;

public class User
{
    public int UserId { get; set; }

    public string GoogleId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>Encrypted Google access token.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Encrypted Google refresh token.</summary>
    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public ICollection<Profile> Profiles { get; set; } = new List<Profile>();

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
