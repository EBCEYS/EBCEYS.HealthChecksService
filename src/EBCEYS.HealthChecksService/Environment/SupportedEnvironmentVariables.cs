using EBCEYS.ContainersEnvironment.ServiceEnvironment;

namespace EBCEYS.HealthChecksService.Environment;

public static class SupportedLogSaverEnvironmentVariables
{
    private const string LogSavePath = "LOG_SAVER_BASE_PATH";

    public static ServiceEnvironmentVariable<string> BaseSavePath { get; } = new(LogSavePath, "/healthchecks/logs");
}

public static class SupportedTelegramEnvironmentVariables
{
    private const string DatabasePathName = "NOTIFICATIONS_TELEGRAM_DB_PATH";
    private const string TelegramBotKeyPathName = "NOTIFICATIONS_TELEGRAM_APIKEY";

    public static ServiceEnvironmentVariable<string> DatabasePath { get; } = new(DatabasePathName, "/db/tg.db");

    public static ServiceEnvironmentVariable<string?> TelegramBotKey { get; } = new(TelegramBotKeyPathName, null,
        "The api key for telegram bot. Null if don't want to have tg notifications.");
}

public static class SupportedHealthChecksEnvironmentVariables
{
    private const string HealthChecksEnabledLabel = "HEALTHCHECKS_LABEL_ENABLED_NAME";
    private const string HealthChecksPortLabel = "HEALTHCHECKS_LABEL_PORT";
    private const string HealthChecksRestartIfUnhealthy = "HEALTHCHECKS_LABEL_RESTART_ON_UNHEALTHY";
    private const string HealthChecksIsEbceysHealthChecksLabel = "HEALTHCHECKS_LABEL_IS_EBCEYS";
    private const string HealthChecksHostNameLabel = "HEALTHCHECKS_LABEL_HOSTNAME";
    private const string SaveLogsIfUnhealthy = "HEALTHCHECKS_SAVE_LOGS_ON_UNHEALTHY";
    private const string HealthChecksProcessorPeriod = "HEALTHCHECKS_PROCESSOR_PERIOD";
    private const string UsePingHealthChecks = "HEALTHCHECKS_USE_PING";
    private const string ServicesHcConfigFilePath = "HEALTHCHECKS_SERVICE_CONFIG_FILE";
    private const string ProcessHcIfContainerStateIs = "HEALTHCHECKS_PROCESS_ONLY_IF_CONTAINER_STATE_IS";

    private const string HealthChecksUnhealthyCount = "HEALTHCHECKS_PROCESS_START_PROCESS_AFTER_UNHEALTHY_COUNT";

    //HC - HealthChecks
    public static ServiceEnvironmentVariable<string> HcEnabledLabel { get; } =
        new(HealthChecksEnabledLabel, "healthchecks.enabled");

    public static ServiceEnvironmentVariable<string> HcPortLabel { get; } =
        new(HealthChecksPortLabel, "healthchecks.port");

    public static ServiceEnvironmentVariable<string> HcRestartIfUnhealthyLabel { get; } =
        new(HealthChecksRestartIfUnhealthy, "healthchecks.restart");

    public static ServiceEnvironmentVariable<string> HcIsEbceysLabel { get; } =
        new(HealthChecksIsEbceysHealthChecksLabel, "healthchecks.isebceys");

    public static ServiceEnvironmentVariable<bool> HcSaveLogs { get; } = new(SaveLogsIfUnhealthy, true);

    public static ServiceEnvironmentVariable<string> HcHostNameLabel { get; } =
        new(HealthChecksHostNameLabel, "healthchecks.hostname");

    public static ServiceEnvironmentVariable<TimeSpan> HcProcessorPeriod { get; } =
        new(HealthChecksProcessorPeriod, TimeSpan.FromSeconds(5.0));

    public static ServiceEnvironmentVariable<bool> HcUsePing { get; } = new(UsePingHealthChecks);

    public static ServiceEnvironmentVariable<string> HcServicesFile { get; } = new(ServicesHcConfigFilePath);

    //TODO: вынести это значение в лейбл контейнера...
    public static ServiceEnvironmentVariable<string> HcProcessOnlyIfContainerState { get; } =
        new(ProcessHcIfContainerStateIs);

    public static ServiceEnvironmentVariable<uint> HcUnhealthyCount { get; } = new(HealthChecksUnhealthyCount, 5);
}

public static class SupportedDockerEnvironmentVariables
{
    private const string DockerConnectionUseDefaultKey = "DOCKER_CONNECTION_USE_DEFAULT";
    private const string DockerConnectionUrlKey = "DOCKER_CONNECTION_URL";
    private const string DockerConnectionDefaultTimeoutKey = "DOCKER_CONNECTION_DEFAULT_TIMEOUT";

    /// <summary>
    ///     The docker connection use default.<br />
    ///     <c>true</c> if <see cref="DockerController" /> should use default connection;<br />
    ///     otherwise <see cref="DockerConnectionUrl" /> will be used.
    /// </summary>
    public static ServiceEnvironmentVariable<bool?> DockerConnectionUseDefault { get; } = new
    (
        DockerConnectionUseDefaultKey,
        true,
        $"If set true: docker client will use localhost connection; otherwise connection from {DockerConnectionUrlKey}"
    );

    /// <summary>
    ///     The docker connection url.
    /// </summary>
    public static ServiceEnvironmentVariable<string> DockerConnectionUrl { get; } = new
    (
        DockerConnectionUrlKey,
        "unix:///var/run/docker.sock",
        $"Param will be ignored if {DockerConnectionUseDefaultKey} set true"
    );

    /// <summary>
    ///     The docker connection timeout.
    /// </summary>
    public static ServiceEnvironmentVariable<TimeSpan?> DockerConnectionTimeout { get; } = new
    (
        DockerConnectionDefaultTimeoutKey,
        TimeSpan.FromSeconds(10.0)
    );
}