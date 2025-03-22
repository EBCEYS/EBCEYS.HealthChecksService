using Docker.DotNet;
using Docker.DotNet.Models;
using EBCEYS.HealthChecksService.Extensions;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor
{
    public class ContainerTaskToProcessInfo(ContainerListResponse container, Task? restartTask, Task<MultiplexedStream>? saveLogTask) : IDisposable
    {
        public ContainerListResponse Container { get; } = container;
        public Task<MultiplexedStream>? GetLogTask { get; } = saveLogTask;
        public Task? RestartTask { get; } = restartTask;

        public void Dispose()
        {
            GetLogTask?.TryDispose();
            RestartTask?.TryDispose();
        }
    }
}
