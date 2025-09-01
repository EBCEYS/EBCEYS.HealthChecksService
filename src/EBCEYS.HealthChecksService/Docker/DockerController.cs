using Docker.DotNet;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Environment;

namespace EBCEYS.HealthChecksService.Docker;

public class DockerController
{
    private readonly DockerClient _client;
    private readonly DockerControllerOptions _opts;

    public DockerController(DockerControllerOptions? optsIn = null)
    {
        _opts = optsIn ?? DockerControllerOptions.CreateFromEnvironment();
        if (_opts.UseDefaultConnection)
            _client = new DockerClientConfiguration(defaultTimeout: _opts.ConnectionTimeout).CreateClient();
        else
            _client =
                new DockerClientConfiguration(_opts.Connection, defaultTimeout: _opts.ConnectionTimeout).CreateClient();
    }

    public async Task<IEnumerable<ContainerListResponse>> GetHealthcheckableContainersAsync(
        CancellationToken token = default)
    {
        IList<ContainerListResponse> response = await _client.Containers.ListContainersAsync(
            new ContainersListParameters
                { All = true }, token);
        var result = response.Where(c => c.Labels.GetLabel<bool>(_opts.SearchHealth.HcEnabledLabel)?.Value == true);
        return result;
    }

    public Task RestartContainerAsync(string id, CancellationToken token = default)
    {
        return _client.Containers.RestartContainerAsync(id, new ContainerRestartParameters
        {
            WaitBeforeKillSeconds = 0
        }, token);
    }

    public async Task<MultiplexedStream> GetContainerLogsAsync(string id, CancellationToken token = default)
    {
        var stream = await _client.Containers.GetContainerLogsAsync(id, true, new ContainerLogsParameters
        {
            Follow = false,
            ShowStderr = true,
            ShowStdout = true,
            Timestamps = true
        }, token);

        return stream;
    }
}

public class DockerControllerOptions(
    bool defaultConnection,
    string conUri,
    TimeSpan timeout,
    SearchHealthChecksOptions searchHealth)
{
    public bool UseDefaultConnection { get; } = defaultConnection;
    public Uri Connection { get; } = new(conUri);
    public TimeSpan ConnectionTimeout { get; } = timeout;
    public SearchHealthChecksOptions SearchHealth { get; } = searchHealth;

    public static DockerControllerOptions CreateFromEnvironment()
    {
        return new DockerControllerOptions(
            SupportedDockerEnvironmentVariables.DockerConnectionUseDefault.Value!.Value,
            SupportedDockerEnvironmentVariables.DockerConnectionUrl.Value!,
            SupportedDockerEnvironmentVariables.DockerConnectionTimeout.Value!.Value,
            SearchHealthChecksOptions.CreateFromEnvironment());
    }
}

public class SearchHealthChecksOptions(string hcEnabled, string hcPort, string isEbceys)
{
    public string HcEnabledLabel { get; } = hcEnabled;
    public string HcPortLabel { get; } = hcPort;
    public string HcIsEbceysLabel { get; } = isEbceys;

    public static SearchHealthChecksOptions CreateFromEnvironment()
    {
        return new SearchHealthChecksOptions(
            SupportedHealthChecksEnvironmentVariables.HcEnabledLabel.Value!,
            SupportedHealthChecksEnvironmentVariables.HcPortLabel.Value!,
            SupportedHealthChecksEnvironmentVariables.HcIsEbceysLabel.Value!);
    }
}