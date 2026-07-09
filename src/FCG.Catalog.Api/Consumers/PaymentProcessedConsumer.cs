using FCG.Catalog.Api.Services;
using FCG.IntegrationEvents.V1;
using MassTransit;
using System.Net.Mail;

namespace FCG.Catalog.Api.Consumers;

public sealed class PaymentProcessedConsumer(
    IUserLibraryService library,
    ILogger<PaymentProcessedConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context) =>
        ConsumeAsync(context.Message, context.CancellationToken);

    public async Task ConsumeAsync(
        PaymentProcessedEvent message,
        CancellationToken cancellationToken)
    {
        Validate(message);

        if (string.Equals(
                message.Status,
                PaymentStatuses.Approved,
                StringComparison.OrdinalIgnoreCase))
        {
            await library.AddApprovedGameAsync(
                message.OrderId,
                message.UserId,
                message.GameId,
                cancellationToken);

            logger.LogInformation(
                "Game added to library after approved payment. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                message.OrderId,
                message.UserId,
                message.GameId);

            return;
        }

        if (string.Equals(
                message.Status,
                PaymentStatuses.Rejected,
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation(
                "Rejected payment did not change library. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                message.OrderId,
                message.UserId,
                message.GameId);

            return;
        }

        logger.LogWarning(
            "Payment result has unknown status. OrderId: {OrderId}, Status: {Status}",
            message.OrderId,
            message.Status);
    }

    private static void Validate(PaymentProcessedEvent message)
    {
        if (message.OrderId == Guid.Empty)
        {
            throw new InvalidPaymentProcessedMessageException(
                "OrderId must not be empty.");
        }

        if (message.ProcessedAtUtc == default)
        {
            throw new InvalidPaymentProcessedMessageException(
                "ProcessedAtUtc must be provided.");
        }

        if (message.UserId == Guid.Empty)
        {
            throw new InvalidPaymentProcessedMessageException(
                "UserId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(message.UserEmail)
            || !MailAddress.TryCreate(message.UserEmail, out _))
        {
            throw new InvalidPaymentProcessedMessageException(
                "UserEmail must be valid.");
        }

        if (message.GameId == Guid.Empty)
        {
            throw new InvalidPaymentProcessedMessageException(
                "GameId must not be empty.");
        }

        if (message.Price <= 0)
        {
            throw new InvalidPaymentProcessedMessageException(
                "Price must be greater than zero.");
        }

        if (!string.Equals(
                message.Status,
                PaymentStatuses.Approved,
                StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                message.Status,
                PaymentStatuses.Rejected,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidPaymentProcessedMessageException(
                "Status must be Approved or Rejected.");
        }
    }
}
