using FCG.Catalog.Api.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FCG.Catalog.Api.Health;

public sealed class RabbitMqHealthCheck(IOptions<RabbitMqOptions> options)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rabbit = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = rabbit.Host,
                Port = rabbit.Port,
                VirtualHost = rabbit.VirtualHost,
                UserName = rabbit.Username,
                Password = rabbit.Password
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "RabbitMQ is unavailable.",
                exception);
        }
    }
}
