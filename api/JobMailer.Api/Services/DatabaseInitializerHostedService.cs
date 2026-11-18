using JobMailer.Data;

namespace JobMailer.Api.Services;

public sealed class DatabaseInitializerHostedService(
    IServiceProvider services,
    ILogger<DatabaseInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DatabaseInitializer.InitializeAsync(services, logger, cancellationToken);
            logger.LogInformation("JobMailer database is ready.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Database initialization failed. Check ConnectionStrings:Default and make sure SQL Server is running.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
