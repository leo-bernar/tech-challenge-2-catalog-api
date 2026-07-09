using FCG.IntegrationEvents.V1;

namespace FCG.Catalog.Api.Messaging;

public interface IOrderPlacedPublisher
{
    Task PublishAsync(
        OrderPlacedEvent message,
        CancellationToken cancellationToken = default);
}
