using FCG.Catalog.Api.Contracts.Games;
using FCG.Catalog.Api.Contracts.Library;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Api.Services;

public static class GameMappings
{
    public static GameResponse ToResponse(Game game) =>
        new(
            game.Id,
            game.Title,
            game.Description,
            game.Developer,
            game.Price,
            game.IsActive,
            game.CreatedAtUtc,
            game.UpdatedAtUtc);

    public static LibraryGameResponse ToLibraryResponse(UserGame userGame) =>
        new(
            userGame.Game.Id,
            userGame.Game.Title,
            userGame.Game.Description,
            userGame.Game.Developer,
            userGame.Game.Price,
            userGame.AcquiredAtUtc);
}
