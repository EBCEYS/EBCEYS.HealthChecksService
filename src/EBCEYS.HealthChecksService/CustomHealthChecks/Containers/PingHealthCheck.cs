using System.Net.NetworkInformation;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Containers
{
    public class PingHealthCheck(IMemoryCache cache) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string? containerId = context.Registration.Tags.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(containerId) || !cache.TryGetValue(containerId, out ContainerListResponse? container) || container == null)
            {
                return HealthCheckResult.Degraded($"Container {containerId ?? "unknown"} not found in cache...");
            }
            string containerName = container.GetName().TrimStart('/');
            bool healthCheckEnabled = container.Labels.GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HCEnabledLabel.Value!)?.Value == true;
            if (!healthCheckEnabled)
            {
                return HealthCheckResult.Healthy($"Container {containerName} is uncheckable!");
            }
            string hostName = container.Labels.GetLabel<string>(SupportedHealthChecksEnvironmentVariables.HCHostNameLabel.Value!)?.Value ?? containerName.TrimStart('/');
            using Ping ping = new();
            PingReply result = await ping.SendPingAsync(hostName, context.Registration.Timeout, cancellationToken: cancellationToken);
            if (result.Status == IPStatus.Success)
            {
                return HealthCheckResult.Healthy("Ping successfully");
            }
            return HealthCheckResult.Unhealthy($"Unsuccessfully ping container! Ping status: {result.Status}");
        }
    }
}
