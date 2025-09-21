using System.Text.Json;
using System.Text.Json.Serialization;
using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;

public abstract class TelegramBotMethodBase
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    protected virtual string Command { get; } = "/base";
    protected virtual string Description { get; } = "description";

    public virtual Task ExecuteAsync(TelegramBotClient botClient, Message message, UpdateType updateType,
        CancellationToken token = default)
    {
        throw new NotImplementedException("You need to implement this method.");
    }

    public string GetHelp()
    {
        return $@"{Command} \- {Description}";
    }

    public static string GetJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, JsonOpts);
    }

    public static string GetJsonForMessage<T>(T obj)
    {
        return $"\n```\n{GetJson(obj)}\n```\n";
    }

    public static TelegramBotMethodBase CreateFromMessage(Message message, IServiceProvider serviceProvider)
    {
        var text = message.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) throw new TelegramBotMessageException(message, "Incorrect text!");

        TelegramBotMethodBase[] commands =
        [
            new StartTgBotCommand(serviceProvider),
            new UnsubTgBotCommand(serviceProvider),
            new GetAllSubsTgBotCommand(serviceProvider),
            new HelpTgBotCommand(serviceProvider, [])
        ];

        return text switch
        {
            "/start" => new StartTgBotCommand(serviceProvider),
            "/help" => new HelpTgBotCommand(serviceProvider, commands),
            "/unsub" => new UnsubTgBotCommand(serviceProvider),
            "/getallsubs" => new GetAllSubsTgBotCommand(serviceProvider),
            _ => throw new TelegramBotMessageException(message, "Unknown command!")
        };
    }
}