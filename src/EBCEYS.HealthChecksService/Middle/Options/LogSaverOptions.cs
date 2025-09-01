using EBCEYS.HealthChecksService.Environment;

namespace EBCEYS.HealthChecksService.Middle.Options;

public class LogSaverOptions(bool saveLogs, string logSaveBasePath, string restartLabel)
{
    public bool SaveLogs { get; } = saveLogs;
    public string LogSaveBasePath { get; } = logSaveBasePath;
    public string RestartLabel { get; } = restartLabel;

    public static LogSaverOptions CreateFromEnvironment()
    {
        return new LogSaverOptions(
            SupportedHealthChecksEnvironmentVariables.HcSaveLogs.Value,
            SupportedLogSaverEnvironmentVariables.BaseSavePath.Value!,
            SupportedHealthChecksEnvironmentVariables.HcRestartIfUnhealthyLabel.Value!);
    }
}