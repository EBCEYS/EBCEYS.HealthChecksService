using Docker.DotNet.Models;
using EBCEYS.HealthChecksService.Docker.Extensions;
using MongoDB.Driver;
using NLog;
using RabbitMQ.Client;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Services
{
    public static class CustomHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder ConfigureServicesHealthChecks(this IHealthChecksBuilder hb, IEnumerable<ContainerListResponse> containers, ServiceHealthChecksOptions? opts, Logger? logger, bool breakIfContainerNotFoundContainer = true)
        {
            if (opts == null || opts.HealthChecks.Count == 0)
            {
                return hb;
            }
            foreach (KeyValuePair<SupportedHealthCheckServices, IEnumerable<ServiceOptions>> hc in opts.HealthChecks)
            {
                foreach (ServiceOptions service in hc.Value)
                {
                    ContainerListResponse? container = containers.GetByNameOrID(service.Container);
                    if (container == null)
                    {
                        logger?.Warn("Container {id} is not found!", service.Container);
                        if (breakIfContainerNotFoundContainer)
                        {
                            throw new InvalidOperationException($"Container {service.Container} not found! Can not confugure HealthChecks!");
                        }
                        continue;
                    }
                    string name = service.ServiceName;
                    IEnumerable<string> tags = [container.ID];
                    switch (hc.Key)
                    {
                        case SupportedHealthCheckServices.RabbitMQ:
                            hb.AddRabbitMQ(sp =>
                            {
                                return new ConnectionFactory()
                                {
                                    Uri = new(service.ConnectionString)
                                }.CreateConnectionAsync();
                            }, name: name, tags: tags);
                            break;
                        case SupportedHealthCheckServices.PostgreSQL:
                            hb.AddNpgSql(service.ConnectionString, name: name, tags: tags);
                            break;
                        case SupportedHealthCheckServices.Redis:
                            hb.AddRedis(service.ConnectionString, name, tags: tags);
                            break;
                        case SupportedHealthCheckServices.MongoDB:
                            hb.AddMongoDb(sp =>
                            {
                                return new MongoClient(service.ConnectionString);
                            }, name: name, tags: tags);
                            break;
                        case SupportedHealthCheckServices.ElasticSearch:
                            hb.AddElasticsearch(service.ConnectionString, name, tags: tags);
                            break;
                        default:
                            logger?.Warn("For service {name} of container {container} type is not supported!", service.ServiceName, service.Container);
                            continue;
                    }
                    logger?.Info("Add {name} healthcheck of service type {type}", name, hc.Key);
                }
            }
            return hb;
        }
    }
}
