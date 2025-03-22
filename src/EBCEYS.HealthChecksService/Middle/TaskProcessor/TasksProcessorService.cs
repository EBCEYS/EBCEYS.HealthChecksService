using Docker.DotNet;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Extensions;
using EBCEYS.HealthChecksService.Middle.Options;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor
{
    public class TasksProcessorService(ILogger<TasksProcessorService> logger, ContainerProcessingQueueStorage queue, LogSaverOptions? opts = null) : BackgroundService
    {
        private readonly LogSaverOptions opts = opts ?? LogSaverOptions.CreateFromEnvironment();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DirectoryInfo baseDir = Directory.CreateDirectory(opts.LogSaveBasePath);
            logger.LogInformation("Starting task processor...");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!queue.TryDequeue(out ContainerTaskToProcessInfo? taskInfo) || taskInfo == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2.0), stoppingToken);
                    continue;
                }
                string containerName = taskInfo.Container.GetName().TrimStart(Path.DirectorySeparatorChar);
                logger.LogInformation("Dequeued task for container {name}", containerName);
                try
                {
                    if (taskInfo.GetLogTask != null && opts.SaveLogs)
                    {
                        DateTimeOffset now = DateTimeOffset.Now;
                        string fileSaveName = $"{now:HH-mm-ss}_{containerName}.log";
                        string nowDateDir = now.ToString("dd-MM-yyyy");
                        string fileCreationPath = Path.Combine(baseDir.FullName, nowDateDir, containerName, fileSaveName);
                        logger.LogDebug("Try to read logs from container {name}", containerName);
                        
                        using MultiplexedStream taskResult = await taskInfo.GetLogTask;

                        await using Stream str = await taskResult.GetLogStream(stoppingToken);
                        Directory.CreateDirectory(Path.GetDirectoryName(fileCreationPath)!);
                        str.Seek(0, SeekOrigin.Begin);
                        await str.WriteToFileAsync(fileCreationPath, stoppingToken);
                        logger.LogInformation("Logs saved to file {filename}", fileCreationPath);
                    }
                    if (taskInfo.RestartTask != null && taskInfo.Container.Labels.GetLabel<bool>(opts.RestartLabel)?.Value == true)
                    {
                        await taskInfo.RestartTask;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on processing container {name}!", containerName);
                }
                taskInfo?.Dispose();
            }

        }
    }
}
