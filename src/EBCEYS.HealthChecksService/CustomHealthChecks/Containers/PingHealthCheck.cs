using System.Net.NetworkInformation;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Containers;

public class PingHealthCheck(IMemoryCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var containerId = context.Registration.Tags.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(containerId) ||
            !cache.TryGetValue(containerId, out ContainerListResponse? container) || container == null)
            return HealthCheckResult.Degraded($"Container {containerId ?? "unknown"} not found in cache...");
        var containerName = container.GetName().TrimStart('/');
        var healthCheckEnabled =
            container.Labels.GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HcEnabledLabel.Value!)?.Value ==
            true;
        if (!healthCheckEnabled) return HealthCheckResult.Healthy($"Container {containerName} is uncheckable!");
        var hostName =
            container.Labels.GetLabel<string>(SupportedHealthChecksEnvironmentVariables.HcHostNameLabel.Value!)
                ?.Value ?? containerName.TrimStart('/');
        using Ping ping = new();
        var result =
            await ping.SendPingAsync(hostName, context.Registration.Timeout, cancellationToken: cancellationToken);
        if (result.Status == IPStatus.Success) return HealthCheckResult.Healthy("Ping successfully");
        return HealthCheckResult.Unhealthy($"Unsuccessfully ping container! Ping status: {result.Status}");
    }
}