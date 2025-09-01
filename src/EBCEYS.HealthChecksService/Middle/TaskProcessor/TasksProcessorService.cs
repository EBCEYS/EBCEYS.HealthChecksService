using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Extensions;
using EBCEYS.HealthChecksService.Middle.Options;

namespace EBCEYS.HealthChecksService.Middle.TaskProcessor;

public class TasksProcessorService(
    ILogger<TasksProcessorService> logger,
    ContainerProcessingQueueStorage queue,
    LogSaverOptions? opts = null) : BackgroundService
{
    private readonly LogSaverOptions _opts = opts ?? LogSaverOptions.CreateFromEnvironment();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var baseDir = Directory.CreateDirectory(_opts.LogSaveBasePath);
        logger.LogInformation("Starting task processor...");
        await foreach (var taskInfo in queue.GetTasksAsync().WithCancellation(stoppingToken))
        {
            stoppingToken.ThrowIfCancellationRequested();
            var containerName = taskInfo.Container.GetName().TrimStart(Path.DirectorySeparatorChar);
            logger.LogInformation("Dequeued task for container {name}", containerName);
            try
            {
                if (taskInfo.GetLogTask != null && _opts.SaveLogs)
                {
                    var now = DateTimeOffset.Now;
                    var fileSaveName = $"{now:HH-mm-ss}_{containerName}.log";
                    var nowDateDir = now.ToString("dd-MM-yyyy");
                    var fileCreationPath = Path.Combine(baseDir.FullName, nowDateDir, containerName, fileSaveName);
                    logger.LogDebug("Try to read logs from container {name}", containerName);

                    using var taskResult = await taskInfo.GetLogTask.Invoke();

                    await using var str = await taskResult.GetLogStream(stoppingToken);
                    Directory.CreateDirectory(Path.GetDirectoryName(fileCreationPath)!);
                    str.Seek(0, SeekOrigin.Begin);
                    await str.WriteToFileAsync(fileCreationPath, stoppingToken);
                    logger.LogInformation("Logs saved to file {filename}", fileCreationPath);
                }

                if (taskInfo.RestartTask != null &&
                    taskInfo.Container.Labels.GetLabel<bool>(_opts.RestartLabel)?.Value == true)
                    await taskInfo.RestartTask.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on processing container {name}!", containerName);
            }
        }
    }
}