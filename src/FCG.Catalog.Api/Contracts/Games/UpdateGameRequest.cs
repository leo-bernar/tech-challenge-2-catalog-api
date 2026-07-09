namespace FCG.Catalog.Api.Contracts.Games;

public sealed record UpdateGameRequest(
    string Title,
    string? Description,
    string? Developer,
    decimal Price);
