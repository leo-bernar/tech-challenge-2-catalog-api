namespace FCG.Catalog.Api.Contracts.Games;

public sealed record CreateGameRequest(
    string Title,
    string? Description,
    string? Developer,
    decimal Price);
