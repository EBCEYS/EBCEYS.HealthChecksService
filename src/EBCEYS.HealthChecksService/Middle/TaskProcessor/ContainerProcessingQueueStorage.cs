using System.Collections.Concurrent;
using Docker.DotNet.Models;
using EBCEYS.HealthChecksService.Docker;
using EBCEYS.HealthChecksService.Docker.Extensions;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor
{
    public class ContainerProcessingQueueStorage(ILogger<ContainerProcessingQueueStorage> logger)
    {
        private readonly ConcurrentQueue<ContainerTaskToProcessInfo> tasks = [];
        public void Enqueue(ContainerTaskToProcessInfo task)
        {
            logger.LogInformation("Add task to process for container {name}", task.Container.GetName());
            tasks.Enqueue(task);
        }
        public void Enqueue(ContainerListResponse container, DockerController docker, CancellationToken token = default)
        {
            Enqueue(new(container, docker.RestartContainerAsync(container.ID, token), docker.GetContainerLogsAsync(container.ID, token)));
        }
        public bool TryDequeue(out ContainerTaskToProcessInfo? task)
        {
            return tasks.TryDequeue(out task);
        }
        public void Clear()
        {
            tasks.Clear();
        }
    }
}
