using Docker.DotNet;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Environment;

namespace EBCEYS.HealthChecksService.Docker
{
    public class DockerController
    {
        private readonly DockerClient client;
        private readonly DockerControllerOptions opts;

        public DockerController(DockerControllerOptions? optsIn = null)
        {
            opts = optsIn ?? DockerControllerOptions.CreateFromEnvironment();
            if (opts.UseDefaultConnection)
            {
                client = new DockerClientConfiguration(defaultTimeout: opts.ConnectionTimeout).CreateClient();
            }
            else
            {
                client = new DockerClientConfiguration(opts.Connection, defaultTimeout: opts.ConnectionTimeout).CreateClient();
            }
        }

        public async Task<IEnumerable<ContainerListResponse>> GetHealthcheckableContainersAsync(CancellationToken token = default)
        {
            IList<ContainerListResponse> response = await client.Containers.ListContainersAsync(new()
            { All = true}, token);
            IEnumerable<ContainerListResponse> result = response.Where(c => c.Labels.GetLabel<bool>(opts.SearchHealth.HCEnabledLabel)?.Value == true);
            return result;
        }
        public Task RestartContainerAsync(string id, CancellationToken token = default)
        {
            return client.Containers.RestartContainerAsync(id, new()
            {
                WaitBeforeKillSeconds = 0
            }, token);
        }
        public async Task<MultiplexedStream> GetContainerLogsAsync(string id, CancellationToken token = default)
        {
            MultiplexedStream stream = await client.Containers.GetContainerLogsAsync(id, true, new()
            {
                Follow = false,
                ShowStderr = true,
                ShowStdout = true,
                Timestamps = true
            }, token);

            return stream;
        }
    }

    public class DockerControllerOptions(bool defaultConnection, string conUri, TimeSpan timeout, SearchHealthChecksOptions searchHealth)
    {
        public bool UseDefaultConnection { get; } = defaultConnection;
        public Uri Connection { get; } = new(conUri);
        public TimeSpan ConnectionTimeout { get; } = timeout;
        public SearchHealthChecksOptions SearchHealth { get; } = searchHealth;

        public static DockerControllerOptions CreateFromEnvironment()
        {
            return new(
                SupportedDockerEnvironmentVariables.DockerConnectionUseDefault.Value!.Value,
                SupportedDockerEnvironmentVariables.DockerConnectionUrl.Value!,
                SupportedDockerEnvironmentVariables.DockerConnectionTimeout.Value!.Value,
                SearchHealthChecksOptions.CreateFromEnvironment());
        }
    }
    public class SearchHealthChecksOptions(string hcEnabled, string hcPort, string isEbceys)
    {
        public string HCEnabledLabel { get; } = hcEnabled;
        public string HCPortLabel { get; } = hcPort;
        public string HCIsEBCEYSLabel { get; } = isEbceys;

        public static SearchHealthChecksOptions CreateFromEnvironment()
        {
            return new(
                SupportedHealthChecksEnvironmentVariables.HCEnabledLabel.Value!, 
                SupportedHealthChecksEnvironmentVariables.HCPortLabel.Value!, 
                SupportedHealthChecksEnvironmentVariables.HCIsEbceysLabel.Value!);
        }
    }
}
