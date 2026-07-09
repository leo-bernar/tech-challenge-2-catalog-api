namespace FCG.Catalog.Api.Contracts.Games;

public sealed record GameResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Developer,
    decimal Price,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
