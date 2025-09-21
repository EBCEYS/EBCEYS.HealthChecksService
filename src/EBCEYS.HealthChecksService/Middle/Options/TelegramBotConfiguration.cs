using EBCEYS.HealthChecksService.Environment;

namespace EBCEYS.HealthChecksService.Middle.Options;

public class TelegramBotConfiguration
{
    public required string ApiKey { get; set; }

    public static TelegramBotConfiguration? CreateFromEnvironment()
    {
        var keyPath = SupportedTelegramEnvironmentVariables.TelegramBotKey.Value;
        if (string.IsNullOrEmpty(keyPath)) return null;

        return new TelegramBotConfiguration
        {
            ApiKey = keyPath
        };
    }
}