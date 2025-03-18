using EBCEYS.HealthChecksService.Environment;

namespace EBCEYS.HealthChecksService.Middle.Options
{
    public class LogSaverOptions(bool saveLogs, string logSaveBasePath, string restartLabel)
    {
        public bool SaveLogs { get; } = saveLogs;
        public string LogSaveBasePath { get; } = logSaveBasePath;
        public string RestartLabel { get; } = restartLabel;

        public static LogSaverOptions CreateFromEnvironment()
        {
            return new(
                SupportedHealthChecksEnvironmentVariables.HCSaveLogs.Value,
                SupportedLogSaverEnvironmentVariables.BaseSavePath.Value!,
                SupportedHealthChecksEnvironmentVariables.HCRestartIfUnhealthyLabel.Value!);
        }
    }
}
