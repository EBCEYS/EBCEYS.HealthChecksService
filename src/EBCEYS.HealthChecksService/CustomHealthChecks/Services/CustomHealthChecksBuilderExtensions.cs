using Docker.DotNet.Models;
using EBCEYS.HealthChecksService.Docker.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using NLog;
using RabbitMQ.Client;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Services;

public static class CustomHealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder ConfigureServicesHealthChecks(this IHealthChecksBuilder hb,
        IEnumerable<ContainerListResponse> containers, ServiceHealthChecksOptions? opts, Logger? logger,
        bool breakIfContainerNotFoundContainer = true)
    {
        if (opts == null || opts.HealthChecks.Count == 0) return hb;
        foreach (var hc in opts.HealthChecks)
        foreach (var service in hc.Value)
        {
            var container = containers.GetByNameOrId(service.Container);
            if (container == null)
            {
                logger?.Warn("Container {id} is not found!", service.Container);
                if (breakIfContainerNotFoundContainer)
                    throw new InvalidOperationException(
                        $"Container {service.Container} not found! Can not confugure HealthChecks!");
                continue;
            }

            var name = service.ServiceName;
            IEnumerable<string> tags = [container.ID];
            switch (hc.Key)
            {
                case SupportedHealthCheckServices.Http:
                    hb.AddAsyncCheck(name, async () =>
                    {
                        using HttpClient client = new()
                        {
                            Timeout = service.Timeout
                        };
                        var response = await client.GetAsync(service.ConnectionString);
                        var status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
                        return new HealthCheckResult(status);
                    }, tags, service.Timeout);
                    break;
                case SupportedHealthCheckServices.RabbitMQ:
                    hb.AddRabbitMQ(async sp =>
                    {
                        return await new ConnectionFactory
                        {
                            Uri = new Uri(service.ConnectionString),
                            RequestedConnectionTimeout = service.Timeout
                        }.CreateConnectionAsync();
                    }, name, tags: tags, timeout: service.Timeout, failureStatus: HealthStatus.Unhealthy);
                    break;
                case SupportedHealthCheckServices.PostgreSql:
                    hb.AddNpgSql(service.ConnectionString, name: name, tags: tags, timeout: service.Timeout,
                        failureStatus: HealthStatus.Unhealthy);
                    break;
                case SupportedHealthCheckServices.Redis:
                    hb.AddRedis(service.ConnectionString, name, tags: tags, timeout: service.Timeout,
                        failureStatus: HealthStatus.Unhealthy);
                    break;
                case SupportedHealthCheckServices.MongoDb:
                    hb.AddMongoDb(sp => { return new MongoClient(service.ConnectionString); }, name: name, tags: tags,
                        timeout: service.Timeout, failureStatus: HealthStatus.Unhealthy);
                    break;
                case SupportedHealthCheckServices.ElasticSearch:
                    hb.AddElasticsearch(service.ConnectionString, name, tags: tags, timeout: service.Timeout);
                    break;
                default:
                    logger?.Warn("For service {name} of container {container} type is not supported!",
                        service.ServiceName, service.Container);
                    continue;
            }

            logger?.Info("Add {name} healthcheck of service type {type}", name, hc.Key);
        }

        return hb;
    }
}