namespace FCG.Catalog.Api.Consumers;

public sealed class InvalidPaymentProcessedMessageException(string message)
    : Exception(message);
