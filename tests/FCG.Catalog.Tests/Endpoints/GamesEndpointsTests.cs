using System.Security.Claims;
using FCG.Catalog.Api;
using FCG.Catalog.Api.Contracts.Games;
using FCG.Catalog.Api.Endpoints;
using FCG.Catalog.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Tests.Endpoints;

public sealed class GamesEndpointsTests
{
    [Fact]
    public async Task Admin_policy_rejects_regular_user_role()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatalogApi(CreateCatalogConfiguration());

        await using var provider = services.BuildServiceProvider();
        var policy = await provider
            .GetRequiredService<IAuthorizationPolicyProvider>()
            .GetPolicyAsync("Admin");
        var authorization = provider
            .GetRequiredService<IAuthorizationService>();

        var userResult = await authorization.AuthorizeAsync(
            CreatePrincipalWithRole("User"),
            resource: null,
            policy!);
        var adminResult = await authorization.AuthorizeAsync(
            CreatePrincipalWithRole("Admin"),
            resource: null,
            policy!);

        Assert.False(userResult.Succeeded);
        Assert.True(adminResult.Succeeded);
    }

    [Theory]
    [InlineData("POST", "/api/games")]
    [InlineData("PUT", "/api/games/{id:guid}")]
    [InlineData("DELETE", "/api/games/{id:guid}")]
    public void Admin_game_routes_require_admin_policy(
        string method,
        string routePattern)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IGameCatalogService, StubGameCatalogService>();
        builder.Services.AddSingleton<IValidator<CreateGameRequest>, InlineValidator<CreateGameRequest>>();
        builder.Services.AddSingleton<IValidator<UpdateGameRequest>, InlineValidator<UpdateGameRequest>>();

        var app = builder.Build();

        app.MapGamesEndpoints();

        var endpoint = ((IEndpointRouteBuilder)app)
            .DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint =>
                string.Equals(
                    endpoint.RoutePattern.RawText?.TrimEnd('/'),
                    routePattern,
                    StringComparison.Ordinal)
                && endpoint.Metadata
                    .GetMetadata<HttpMethodMetadata>()!
                    .HttpMethods
                    .Contains(method));

        var authorization = endpoint.Metadata
            .GetOrderedMetadata<IAuthorizeData>();

        Assert.Contains(authorization, data => data.Policy == "Admin");
    }

    private static IConfiguration CreateCatalogConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CatalogDatabase"] =
                    "Server=localhost;Database=FcgCatalogDb;User Id=sa;Password=Password123!;TrustServerCertificate=True;Encrypt=False",
                ["Jwt:Issuer"] = "FCG",
                ["Jwt:Audience"] = "FCG",
                ["Jwt:Key"] = "0123456789abcdef0123456789abcdef",
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:PaymentProcessedQueue"] = "catalog-payment-processed"
            })
            .Build();

    private static ClaimsPrincipal CreatePrincipalWithRole(string role) =>
        new(new ClaimsIdentity(
            [new Claim("role", role)],
            authenticationType: "Test",
            nameType: "name",
            roleType: "role"));

    private sealed class StubGameCatalogService : IGameCatalogService
    {
        public Task<GameResponse> CreateAsync(
            CreateGameRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<GameResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<GameResponse>> ListAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<GameResponse> UpdateAsync(
            Guid id,
            UpdateGameRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
