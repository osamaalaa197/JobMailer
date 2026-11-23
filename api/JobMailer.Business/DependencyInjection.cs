using System.Text;
using JobMailer.Business.Abstractions;
using JobMailer.Business.Options;
using JobMailer.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobMailer.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GoogleOptions>()
            .Bind(configuration.GetSection(GoogleOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.ClientId), "Google:ClientId is required.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.ClientSecret), "Google:ClientSecret is required.")
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.Key), "Jwt:Key is required.")
            .Validate(x => Encoding.UTF8.GetByteCount(x.Key) >= 32, "Jwt:Key must be at least 32 characters long.")
            .ValidateOnStart();

        services.AddOptions<TrackingOptions>()
            .Bind(configuration.GetSection(TrackingOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
                {
                    var webUrl = configuration["Web:Url"];
                    options.PublicBaseUrl = string.IsNullOrWhiteSpace(webUrl)
                        ? "http://localhost:5080"
                        : webUrl;
                }
            });

        services.AddHttpClient("google", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpClient("ai", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(45);
        });

        services.AddScoped<IAiEmailEnhancer, OpenAiCompatibleEmailEnhancer>();

        services.AddSingleton<ITokenProtector, DataProtectionTokenProtector>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITrackingService, TrackingService>();

        return services;
    }
}
