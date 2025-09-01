using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.ContainersEnvironment.HealthChecks;
using EBCEYS.ContainersEnvironment.HealthChecks.Environment;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using EBCEYS.HealthChecksService.CustomHealthChecks.Containers;
using EBCEYS.HealthChecksService.CustomHealthChecks.Services;
using EBCEYS.HealthChecksService.Docker;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Environment;
using EBCEYS.HealthChecksService.Middle;
using EBCEYS.HealthChecksService.Middle.TaskProcessor;
using NLog;
using NLog.Web;
using PingHealthCheck = EBCEYS.HealthChecksService.CustomHealthChecks.Containers.PingHealthCheck;

namespace EBCEYS.HealthChecksService;

public class Program
{
    public const string EbceysHealthChecksPostfix = "-ebceys-healthz";
    public const string PingHealthChecksPostfix = "-ping";
    private static DockerController Docker { get; } = new();

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureConfiguration(builder);

        await ConfigureServices(builder);

        ConfigureLogging(builder);

        var app = builder.Build();

        app.UseRouting();
        app.ConfigureHealthChecks();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapHealthChecksUI(s => { s.UIPath = "/healthstatuses"; });

        app.Run();
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Configuration.AddJsonFile("appsettings.json", false);
        builder.Configuration.AddEnvironmentVariables();
    }

    private static async Task ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddRouting(r => r.LowercaseUrls = true);

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<DockerController>(Docker);
        builder.Services.AddHostedService<DockerCacheProcessorService>();
        builder.Services.AddSingleton<ContainerProcessingQueueStorage>();
        builder.Services.AddHostedService<TasksProcessorService>();
        builder.Services.AddHostedService<HealthChecksProcessorService>();

        var containers = await Docker.GetHealthcheckableContainersAsync();
        var healthCheckBuilder = builder.Services.ConfigureHealthChecks()!;
        foreach (var container in containers)
        {
            var isEbceysLabel = SupportedHealthChecksEnvironmentVariables.HcIsEbceysLabel.Value!;
            if (container.Labels.GetLabel<bool>(isEbceysLabel)?.Value == true)
            {
                var healthCheckName = $"{container.GetName()}{EbceysHealthChecksPostfix}";
                healthCheckBuilder.AddCheck<EbceysHealthCheck>(healthCheckName, timeout: TimeSpan.FromSeconds(10.0),
                    tags: [container.ID]);
            }

            if (SupportedHealthChecksEnvironmentVariables.HcUsePing.Value)
            {
                var pingHealthCheckName = $"{container.GetName()}{PingHealthChecksPostfix}";
                healthCheckBuilder.AddCheck<PingHealthCheck>(pingHealthCheckName, timeout: TimeSpan.FromSeconds(10.0),
                    tags: [container.ID]);
            }
        }

        var serviceOptions =
            ServiceHealthChecksOptions.CreateFromJsonFile(
                SupportedHealthChecksEnvironmentVariables.HcServicesFile.Value);
        healthCheckBuilder.ConfigureServicesHealthChecks(containers, serviceOptions,
            LogManager.GetCurrentClassLogger());
        builder.Services.AddHealthChecksUI(setup =>
        {
            UriBuilder uriB = new()
            {
                Scheme = "http",
                Host = "localhost",
                Port = HealthChecksEnvironmentVariables.HealthChecksPort.Value!.Value,
                Path = ServiceHealthChecksRoutes.HealthzStatusRoute.TrimStart('/')
            };
            var route = uriB.ToString();
            setup.AddHealthCheckEndpoint("containers", route);
            setup.MaximumHistoryEntriesPerEndpoint(5);
            setup.SetEvaluationTimeInSeconds(Convert.ToInt32(SupportedHealthChecksEnvironmentVariables.HcProcessorPeriod
                .Value.TotalSeconds));
        }).AddInMemoryStorage();


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddNLogWeb(logger.Factory.Configuration);
    }
}