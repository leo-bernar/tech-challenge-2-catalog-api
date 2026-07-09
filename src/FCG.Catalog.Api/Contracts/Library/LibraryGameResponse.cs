namespace FCG.Catalog.Api.Contracts.Library;

public sealed record LibraryGameResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Developer,
    decimal Price,
    DateTime AcquiredAtUtc);
