using EBCEYS.ContainersEnvironment.ServiceEnvironment;

namespace EBCEYS.HealthChecksService.Environment
{
    public static class SupportedLogSaverEnvironmentVariables
    {
        private const string logSavePath = "LOG_SAVER_BASE_PATH";

        public static ServiceEnvironmentVariable<string> BaseSavePath { get; } = new(logSavePath, "/healthchecks/logs");
    }
    public static class SupportedHealthChecksEnvironmentVariables
    {
        private const string healthChecksEnabledLabel = "HEALTHCHECKS_LABEL_ENABLED_NAME";
        private const string healthChecksPortLabel = "HEALTHCHECKS_LABEL_PORT";
        private const string healthChecksRestartIfUnhealthy = "HEALTHCHECKS_LABEL_RESTART_ON_UNHEALTHY";
        private const string healthChecksIsEBCEYSHealthChecksLabel = "HEALTHCHECKS_LABEL_IS_EBCEYS";
        private const string healthChecksHostNameLabel = "HEALTHCHECKS_LABEL_HOSTNAME";
        private const string saveLogsIfUnhealthy = "HEALTHCHECKS_SAVE_LOGS_ON_UNHEALTHY";
        private const string healthChecksProcessorPeriod = "HEALTHCHECKS_PROCESSOR_PERIOD";
        private const string usePingHealthChecks = "HEALTHCHECKS_USE_PING";
        private const string servicesHCConfigFilePath = "HEALTHCHECKS_SERVICE_CONFIG_FILE";
        private const string processHCIfContainerIsRunning = "HEALTHCHECKS_PROCESS_ONLY_IF_CONTAINER_RUNNING";
        private const string healthChecksUnhealthyCount = "HEALTHCHECKS_PROCESS_START_PROCESS_AFTER_UNHEALTHY_COUNT";
        //HC - HealthChecks
        public static ServiceEnvironmentVariable<string> HCEnabledLabel { get; } = new(healthChecksEnabledLabel, "healthchecks.enabled");
        public static ServiceEnvironmentVariable<string> HCPortLabel { get; } = new(healthChecksPortLabel, "healthchecks.port");
        public static ServiceEnvironmentVariable<string> HCRestartIfUnhealthyLabel { get; } = new(healthChecksRestartIfUnhealthy, "healthchecks.restart");
        public static ServiceEnvironmentVariable<string> HCIsEbceysLabel { get; } = new(healthChecksIsEBCEYSHealthChecksLabel, "healthchecks.isebceys");
        public static ServiceEnvironmentVariable<bool> HCSaveLogs { get; } = new(saveLogsIfUnhealthy, true);
        public static ServiceEnvironmentVariable<string> HCHostNameLabel { get; } = new(healthChecksHostNameLabel, "healthchecks.hostname");
        public static ServiceEnvironmentVariable<TimeSpan?> HCProcessorPeriod { get; } = new(healthChecksProcessorPeriod, TimeSpan.FromSeconds(5.0));
        public static ServiceEnvironmentVariable<bool> HCUsePing { get; } = new(usePingHealthChecks, false);
        public static ServiceEnvironmentVariable<string> HCServicesFile { get; } = new(servicesHCConfigFilePath, null);
        public static ServiceEnvironmentVariable<bool> HCProcessOnlyIfRunnung { get; } = new(processHCIfContainerIsRunning, true);
        public static ServiceEnvironmentVariable<uint> HCUnhealthyCount { get; } = new(healthChecksUnhealthyCount, 5);
    }
    public static class SupportedDockerEnvironmentVariables
    {
        private const string dockerConnectionUseDefaultKey = "DOCKER_CONNECTION_USE_DEFAULT";
        private const string dockerConnectionUrlKey = "DOCKER_CONNECTION_URL";
        private const string dockerConnectionDefaultTimeoutKey = "DOCKER_CONNECTION_DEFAULT_TIMEOUT";

        /// <summary>
        /// The docker connection use default.<br/>
        /// <c>true</c> if <see cref="DockerController"/> should use default connection;<br/>
        /// otherwise <see cref="DockerConnectionUrl"/> will be used.
        /// </summary>
        public static ServiceEnvironmentVariable<bool?> DockerConnectionUseDefault { get; } = new
            (
            dockerConnectionUseDefaultKey,
            true,
            $"If set true: docker client will use localhost connection; otherwise connection from {dockerConnectionUrlKey}"
            );
        /// <summary>
        /// The docker connection url.
        /// </summary>
        public static ServiceEnvironmentVariable<string> DockerConnectionUrl { get; } = new
            (
            dockerConnectionUrlKey,
            "unix:///var/run/docker.sock",
            $"Param will be ignored if {dockerConnectionUseDefaultKey} set true"
            );
        /// <summary>
        /// The docker connection timeout.
        /// </summary>
        public static ServiceEnvironmentVariable<TimeSpan?> DockerConnectionTimeout { get; } = new
            (
            dockerConnectionDefaultTimeoutKey,
            TimeSpan.FromSeconds(10.0)
            );
    }
}
