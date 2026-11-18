namespace JobMailer.Data.Entities;

public class Template
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
}
