using EBCEYS.HealthChecksService.Docker;
using Microsoft.Extensions.Caching.Memory;

namespace EBCEYS.HealthChecksService.Middle;

public class DockerCacheProcessorService(
    ILogger<DockerCacheProcessorService> logger,
    DockerController docker,
    IMemoryCache cache) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var containers = await docker.GetHealthcheckableContainersAsync(stoppingToken);
                foreach (var container in containers) cache.Set(container.ID, container);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on cache processing!");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}