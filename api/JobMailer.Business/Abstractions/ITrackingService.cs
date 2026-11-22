namespace JobMailer.Business.Abstractions;

public interface ITrackingService
{
    Task RecordOpenAsync(string trackingToken, CancellationToken cancellationToken = default);
}
