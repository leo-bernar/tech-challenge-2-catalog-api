using FCG.IntegrationEvents.V1;
using MassTransit;

namespace FCG.Catalog.Api.Messaging;

public sealed class MassTransitOrderPlacedPublisher(IPublishEndpoint publishEndpoint)
    : IOrderPlacedPublisher
{
    public Task PublishAsync(
        OrderPlacedEvent message,
        CancellationToken cancellationToken = default) =>
        publishEndpoint.Publish(message, cancellationToken);
}
