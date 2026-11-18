namespace JobMailer.Data.Entities;

public class Profile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? CvFileName { get; set; }

    /// <summary>Base64 encoded CV contents (PDF/DOC/DOCX).</summary>
    public string? CvBase64 { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
