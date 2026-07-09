using FCG.Catalog.Api.Messaging;
using FCG.IntegrationEvents.V1;

namespace FCG.Catalog.Tests;

public sealed class RecordingOrderPlacedPublisher : IOrderPlacedPublisher
{
    public List<OrderPlacedEvent> Messages { get; } = [];

    public Task PublishAsync(
        OrderPlacedEvent message,
        CancellationToken cancellationToken = default)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }
}
