using System.Collections.Concurrent;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Docker;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using EBCEYS.HealthChecksService.Middle.TaskProcessor;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.Middle
{
    public class HealthChecksProcessorService(ILogger<HealthChecksProcessorService> logger, HealthCheckService health, ContainerProcessingQueueStorage queue, DockerController docker, IMemoryCache cache) : BackgroundService
    {
        private const string runningStatus = "running";
        private readonly ConcurrentDictionary<string, HealthStatus> containerHealths = [];
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting healthchecks processor...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    HealthReport healthResult = await health.CheckHealthAsync(stoppingToken);
                    foreach (KeyValuePair<string, HealthReportEntry> healthStatus in healthResult.Entries)
                    {
                        if (cache.TryGetValue(healthStatus.Value.Tags.FirstOrDefault() ?? "unknown", out ContainerListResponse? container) && container != null)
                        {
                            bool healthCheckEnabled = container.Labels.GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HCEnabledLabel.Value!)?.Value == true;
                            if (!healthCheckEnabled)
                            {
                                continue;
                            }
                            if (healthStatus.Value.Status == HealthStatus.Unhealthy)
                            {
                                bool shouldRunning = SupportedHealthChecksEnvironmentVariables.HCProcessOnlyIfRunnung.Value;
                                if (shouldRunning && string.Compare(container.Status, runningStatus, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    continue;
                                }
                                if (healthStatus.Key.EndsWith(Program.ebceysHealthChecksPostfix) &&
                                    healthStatus.Value.Data.TryGetValue(EBCEYS.ContainersEnvironment.HealthChecks.PingServiceHealthStatusInfo.HealthCheckName, out object? val) &&
                                    val != null &&
                                    val is UIHealthReportEntry entry)
                                {
                                    logger.LogWarning("Container {name} is UNHEALTHY with description: {desc} exception: {ex}", container.GetName(), entry.Description, entry.Exception);
                                }
                                logger.LogInformation("Container {name} is unhealthy", container.GetName());

                                if (containerHealths.TryGetValue(container.ID, out HealthStatus prevStatus) && prevStatus != HealthStatus.Unhealthy)
                                {
                                    queue.Enqueue(container, docker, stoppingToken);
                                }
                            }

                            containerHealths[container.ID] = healthStatus.Value.Status;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on healthchecks processing!");
                }
                await Task.Delay(SupportedHealthChecksEnvironmentVariables.HCProcessorPeriod.Value!.Value, stoppingToken);
            }
        }
    }
}
