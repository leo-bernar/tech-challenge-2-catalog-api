using FCG.Catalog.Api;
using FCG.Catalog.Api.Endpoints;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddCatalogApi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => TypedResults.Ok(new
{
    service = "FCG Catalog API",
    status = "running"
}))
.WithName("GetServiceStatus")
.WithOpenApi();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapGamesEndpoints();
app.MapLibraryEndpoints();

await app.Services.InitialiseCatalogDatabaseAsync();
await app.RunAsync();

public partial class Program;
