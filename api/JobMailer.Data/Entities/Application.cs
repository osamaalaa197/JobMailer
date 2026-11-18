namespace JobMailer.Data.Entities;

public class Application
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public int? ProfileId { get; set; }

    public string ProfileTitle { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string? JobTitle { get; set; }

    public string ToEmail { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}
