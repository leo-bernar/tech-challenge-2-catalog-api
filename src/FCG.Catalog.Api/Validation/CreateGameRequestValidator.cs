using FCG.Catalog.Api.Contracts.Games;
using FCG.Catalog.Domain.Entities;
using FluentValidation;

namespace FCG.Catalog.Api.Validation;

public sealed class CreateGameRequestValidator
    : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(Game.MaxTitleLength);
        RuleFor(request => request.Description)
            .MaximumLength(Game.MaxDescriptionLength);
        RuleFor(request => request.Developer)
            .MaximumLength(Game.MaxDeveloperLength);
        RuleFor(request => request.Price)
            .GreaterThan(0);
    }
}
