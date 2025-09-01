using System.Collections.Concurrent;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.ContainersEnvironment.HealthChecks;
using EBCEYS.HealthChecksService.Docker;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using EBCEYS.HealthChecksService.Middle.TaskProcessor;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.Middle;

public class HealthChecksProcessorService(
    ILogger<HealthChecksProcessorService> logger,
    HealthCheckService health,
    ContainerProcessingQueueStorage queue,
    DockerController docker,
    IMemoryCache cache) : BackgroundService
{
    private readonly ConcurrentDictionary<string, ContainerHealthInfo> _containerHealths = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting healthchecks processor...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var healthResult = await health.CheckHealthAsync(stoppingToken);
                foreach (var (healthStatusKey, reportEntry) in healthResult.Entries)
                {
                    if (!cache.TryGetValue(reportEntry.Tags.FirstOrDefault() ?? "unknown",
                            out ContainerListResponse? container) || container == null) continue;
                    var healthCheckEnabled =
                        container.Labels
                            .GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HcEnabledLabel.Value!)
                            ?.Value == true;
                    if (!healthCheckEnabled) continue;
                    _containerHealths.TryAdd(healthStatusKey, new ContainerHealthInfo());
                    if (reportEntry.Status == HealthStatus.Unhealthy)
                    {
                        var runningState = SupportedHealthChecksEnvironmentVariables.HcProcessOnlyIfContainerState
                            .Value;
                        logger.LogDebug(
                            "Should state {shouldRunning} Container {name} status is {status} state {state}",
                            runningState, container.GetName(), container.Status, container.State);
                        if (runningState != null && string.Compare(container.State, runningState,
                                StringComparison.InvariantCultureIgnoreCase) != 0) continue;
                        if (healthStatusKey.EndsWith(Program.EbceysHealthChecksPostfix) &&
                            reportEntry.Data.TryGetValue(PingServiceHealthStatusInfo.HealthCheckName,
                                out var val) &&
                            val is UIHealthReportEntry entry)
                            logger.LogWarning(
                                "Container {name} is UNHEALTHY with description: {desc} exception: {ex}",
                                container.GetName(), entry.Description, entry.Exception);
                        logger.LogInformation("Container {name} is unhealthy", container.GetName());

                        if (_containerHealths.TryGetValue(healthStatusKey, out var healthInfo) &&
                            !healthInfo.Processed && healthInfo.UnhealthyCount >
                            SupportedHealthChecksEnvironmentVariables.HcUnhealthyCount.Value)
                        {
                            healthInfo.Processed = true;
                            await queue.Enqueue(container, docker, stoppingToken);
                        }
                    }

                    if (_containerHealths.TryGetValue(healthStatusKey, out var info))
                        if (reportEntry.Status == HealthStatus.Unhealthy)
                        {
                            info.PlusOne();
                            logger.LogDebug(
                                "Current unhealthy count for container {name} is {count} and container processed is {processed}",
                                container.GetName(), info.UnhealthyCount, info.Processed);
                            continue;
                        }

                    _containerHealths[healthStatusKey].SetDefault();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on healthchecks processing!");
            }

            await Task.Delay(SupportedHealthChecksEnvironmentVariables.HcProcessorPeriod.Value, stoppingToken);
        }
    }

    private class ContainerHealthInfo
    {
        public uint UnhealthyCount { get; private set; }
        public bool Processed { get; set; }

        public void SetDefault()
        {
            UnhealthyCount = 0;
            Processed = false;
        }

        public void PlusOne()
        {
            UnhealthyCount++;
        }
    }
}