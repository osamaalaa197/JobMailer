using JobMailer.Business.Abstractions;
using JobMailer.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace JobMailer.Business.Services;

public sealed class TrackingService(
    IApplicationRepository applications,
    ILogger<TrackingService> logger) : ITrackingService
{
    public async Task RecordOpenAsync(
        string trackingToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken) || trackingToken.Length > 64)
        {
            return;
        }

        var recorded = await applications.RecordOpenAsync(
            trackingToken.Trim(),
            DateTime.UtcNow,
            cancellationToken);

        if (!recorded)
        {
            logger.LogDebug("Received an open event for an unknown tracking token.");
        }
    }
}
