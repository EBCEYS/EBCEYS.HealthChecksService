using Docker.DotNet;
using Docker.DotNet.Models;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor;

public class ContainerTaskToProcessInfo(
    ContainerListResponse container,
    Func<Task>? restartTask,
    Func<Task<MultiplexedStream>>? saveLogTask)
{
    public ContainerListResponse Container { get; } = container;
    public Func<Task<MultiplexedStream>>? GetLogTask { get; } = saveLogTask;
    public Func<Task>? RestartTask { get; } = restartTask;
}