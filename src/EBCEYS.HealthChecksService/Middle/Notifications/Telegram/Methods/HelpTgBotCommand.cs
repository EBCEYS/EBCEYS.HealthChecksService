using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;

public class HelpTgBotCommand(IServiceProvider serviceProvider, IReadOnlyCollection<TelegramBotMethodBase> commands)
    : TelegramBotMethodBase
{
    private readonly ILogger<HelpTgBotCommand>
        _logger = serviceProvider.GetRequiredService<ILogger<HelpTgBotCommand>>();

    protected override string Command { get; } = "/help";
    protected override string Description { get; } = "Displays the available commands";

    public override async Task ExecuteAsync(TelegramBotClient botClient, Message message, UpdateType updateType,
        CancellationToken token = default)
    {
        var username = message.Chat.Username ?? throw new ArgumentException(nameof(message.Chat));
        _logger.LogInformation("Try execute {command} for user {user}", GetType().Name, username);

        var helps = commands.Select(c => c.GetHelp());
        await botClient.SendMessage(message.Chat.Id, string.Join('\n', helps), ParseMode.MarkdownV2,
            cancellationToken: token);
    }
}