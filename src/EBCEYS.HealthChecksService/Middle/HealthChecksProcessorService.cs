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
        private readonly ConcurrentDictionary<string, ContainerHealthInfo> containerHealths = [];
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
                                logger.LogDebug("Should running is {shouldRunning} Container {name} status is {status} state {state}", shouldRunning, container.GetName(), container.Status, container.State);
                                if (shouldRunning && string.Compare(container.State, runningStatus, StringComparison.InvariantCultureIgnoreCase) != 0)
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

                                if (containerHealths.TryGetValue(container.ID, out ContainerHealthInfo? healthInfo) && healthInfo != null && healthInfo.Status != HealthStatus.Unhealthy&& healthInfo.UnhealthyCount > SupportedHealthChecksEnvironmentVariables.HCUnhealthyCount.Value)
                                {
                                    healthInfo.Status = HealthStatus.Unhealthy;
                                    queue.Enqueue(container, docker, stoppingToken);
                                }
                            }

                            if (containerHealths.TryGetValue(container.ID, out _))
                            {
                                if (healthStatus.Value.Status == HealthStatus.Unhealthy)
                                {
                                    containerHealths[container.ID].PlusOne();
                                    continue;
                                }
                            }
                            containerHealths[container.ID] ??= new();
                            containerHealths[container.ID].SetDefault();
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
        private class ContainerHealthInfo
        {
            public int UnhealthyCount {get;set;} = 0;
            public HealthStatus Status {get;set;} = HealthStatus.Healthy;
            public void SetDefault(HealthStatus status = HealthStatus.Healthy)
            {
                UnhealthyCount = 0;
                Status = status;
            }
            public void PlusOne()
            {
                UnhealthyCount++;
            }
        }
    }
}
