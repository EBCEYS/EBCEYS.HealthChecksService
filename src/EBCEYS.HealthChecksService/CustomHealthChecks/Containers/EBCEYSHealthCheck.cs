using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.ContainersEnvironment.HealthChecks;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Containers;

public class EbceysHealthCheck(IMemoryCache cache) : IHealthCheck
{
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(null)
        }
    };

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!cache.TryGetValue(context.Registration.Tags.First(), out ContainerListResponse? container) ||
            container == null) return HealthCheckResult.Degraded("Containers info has expired in cache!");
        var containerName = container.GetName().TrimStart('/');
        var healthCheckEnabled =
            container.Labels.GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HcEnabledLabel.Value!)?.Value ==
            true;
        var isEbceysHealthChecks =
            container.Labels.GetLabel<bool>(SupportedHealthChecksEnvironmentVariables.HcIsEbceysLabel.Value!)?.Value ==
            true;
        if (!healthCheckEnabled || !isEbceysHealthChecks)
            return HealthCheckResult.Healthy($"Container {containerName} is uncheckable!");
        var hostName =
            container.Labels.GetLabel<string>(SupportedHealthChecksEnvironmentVariables.HcHostNameLabel.Value!)
                ?.Value ?? containerName.TrimStart('/');
        var port =
            container.Labels.GetLabel<int>(SupportedHealthChecksEnvironmentVariables.HcPortLabel.Value!)?.Value ?? 8080;
        UriBuilder uriBuilder = new()
        {
            Scheme = "http",
            Host = hostName,
            Port = port,
            Path = ServiceHealthChecksRoutes.HealthzStatusRoute.TrimStart('/')
        };
        using HttpClient client = new()
        {
            Timeout = context.Registration.Timeout
        };
        try
        {
            var response = await client.GetAsync(uriBuilder.Uri, cancellationToken);

            var health = await response.Content.ReadFromJsonAsync<UIHealthReport>(_jsonOpts, cancellationToken);
            StringBuilder sb = new();
            var resultName = $"{containerName} healthz result:";
            sb.AppendLine(resultName);
            if (health?.Entries != null)
                foreach (var entry in health.Entries)
                    sb.AppendLine($"{entry.Key} : {entry.Value.Description} duration: {entry.Value.Duration}");

            var data = GetData(health?.Entries);
            if (health == null || health.Status == UIHealthStatus.Unhealthy)
                return HealthCheckResult.Unhealthy(sb.ToString(), data: data);
            return HealthCheckResult.Healthy(sb.ToString(), data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Error on sending request to container {containerName}", ex);
        }
    }

    private static Dictionary<string, object>? GetData(IReadOnlyDictionary<string, UIHealthReportEntry>? entries)
    {
        return entries?.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).ToDictionary();
    }
}