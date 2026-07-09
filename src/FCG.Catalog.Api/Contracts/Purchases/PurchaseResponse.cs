namespace FCG.Catalog.Api.Contracts.Purchases;

public sealed record PurchaseResponse(
    Guid OrderId,
    Guid GameId,
    decimal Price,
    string Status);
