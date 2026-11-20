using JobMailer.Data.Entities;

namespace JobMailer.Data;

public static class TemplateDefaults
{
    public static void AddDefaults(User user)
    {
        user.Templates.Add(new Template
        {
            Key = "Full Stack",
            Subject = "Application for {{JobTitle}} - {{FullName}}",
            Body = "Dear {{ReceiverName}},\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. " +
                   "I specialize in full-stack web applications with React and .NET and would welcome the chance to contribute to your team.\n\n" +
                   "My CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}"
        });

        user.Templates.Add(new Template
        {
            Key = "GIS",
            Subject = "Application for {{JobTitle}} - {{FullName}}",
            Body = "Dear {{ReceiverName}},\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. " +
                   "I specialize in GIS solutions, spatial data and web mapping and would welcome the chance to contribute to your team.\n\n" +
                   "My CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}"
        });
    }
}
