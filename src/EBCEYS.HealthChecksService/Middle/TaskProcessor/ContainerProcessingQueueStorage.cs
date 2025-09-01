using System.Threading.Channels;
using Docker.DotNet.Models;
using EBCEYS.HealthChecksService.Docker;
using EBCEYS.HealthChecksService.Docker.Extensions;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor;

public class ContainerProcessingQueueStorage(ILogger<ContainerProcessingQueueStorage> logger)
{
    private readonly Channel<ContainerTaskToProcessInfo> _tasks = Channel.CreateUnbounded<ContainerTaskToProcessInfo>();

    private async Task Enqueue(ContainerTaskToProcessInfo task)
    {
        logger.LogInformation("Add task to process for container {name}", task.Container.GetName());
        await _tasks.Writer.WriteAsync(task);
    }

    public async Task Enqueue(ContainerListResponse container, DockerController docker,
        CancellationToken token = default)
    {
        await Enqueue(new ContainerTaskToProcessInfo(container, () => docker.RestartContainerAsync(container.ID, token),
            () => docker.GetContainerLogsAsync(container.ID, token)));
    }

    public IAsyncEnumerable<ContainerTaskToProcessInfo> GetTasksAsync()
    {
        return _tasks.Reader.ReadAllAsync();
    }
}