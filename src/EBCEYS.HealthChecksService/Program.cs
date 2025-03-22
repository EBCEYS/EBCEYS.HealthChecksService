using Docker.DotNet.Models;
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

namespace EBCEYS.HealthChecksService
{
    public class Program
    {
        public const string ebceysHealthChecksPostfix = "-ebceys-healthz";
        public const string pingHealthChecksPostfix = "-ping";
        private static DockerController Docker { get; } = new();
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            ConfigureConfiguration(builder);

            await ConfigureServices(builder);

            ConfigureLogging(builder);

            WebApplication app = builder.Build();

            app.UseRouting();
            app.ConfigureHealthChecks();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapHealthChecksUI(s =>
            {
                s.UIPath = "/healthstatuses";
            });

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
            builder.Services.ConfigureHealthChecks();

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<DockerController>(Docker);
            builder.Services.AddHostedService<DockerCacheProcessorService>();
            builder.Services.AddSingleton<ContainerProcessingQueueStorage>();
            builder.Services.AddHostedService<TasksProcessorService>();
            builder.Services.AddHostedService<HealthChecksProcessorService>();

            IEnumerable<ContainerListResponse> containers = await Docker.GetHealthcheckableContainersAsync();
            IHealthChecksBuilder healthCheckBuilder = builder.Services.AddHealthChecks();
            foreach (ContainerListResponse container in containers)
            {
                string healthCheckName = $"{container.GetName()}{ebceysHealthChecksPostfix}";
                healthCheckBuilder.AddCheck<EBCEYSHealthCheck>(healthCheckName, timeout: TimeSpan.FromSeconds(10.0), tags: [container.ID]);
                if (SupportedHealthChecksEnvironmentVariables.HCUsePing.Value)
                {
                    string pingHealthCheckName = $"{container.GetName()}{pingHealthChecksPostfix}";
                    healthCheckBuilder.AddCheck<PingHealthCheck>(pingHealthCheckName, timeout: TimeSpan.FromSeconds(10.0), tags: [container.ID]);
                }
            }
            //TODO: now test with some service which can send unhealthy to check healths
            ServiceHealthChecksOptions? serviceOptions = ServiceHealthChecksOptions.CreateFromJsonFile(SupportedHealthChecksEnvironmentVariables.HCServicesFile.Value);
            healthCheckBuilder.ConfigureServicesHealthChecks(containers, serviceOptions, LogManager.GetCurrentClassLogger(), true);
            builder.Services.AddHealthChecksUI(setup =>
            {
                UriBuilder uriB = new()
                {
                    Scheme = "http",
                    Host = "localhost",
                    Port = EBCEYS.ContainersEnvironment.HealthChecks.Environment.HealthChecksEnvironmentVariables.HealthChecksPort.Value!.Value,
                    Path = EBCEYS.ContainersEnvironment.HealthChecks.ServiceHealthChecksRoutes.HealthzStatusRoute.TrimStart('/')
                };
                string route = uriB.ToString();
                setup.AddHealthCheckEndpoint("containers", route);
                setup.MaximumHistoryEntriesPerEndpoint(5);
                setup.SetEvaluationTimeInSeconds(Convert.ToInt32(SupportedHealthChecksEnvironmentVariables.HCProcessorPeriod.Value!.Value.TotalSeconds));
            }).AddInMemoryStorage();


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb(logger.Factory.Configuration);
        }
    }
}
