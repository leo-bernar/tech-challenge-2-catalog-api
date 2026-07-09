using System.ComponentModel.DataAnnotations;

namespace FCG.Catalog.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string Key { get; init; } = string.Empty;
}
