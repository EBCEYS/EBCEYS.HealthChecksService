using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;

public class UnsubTgBotCommand(IServiceProvider serviceProvider) : TelegramBotMethodBase
{
    private readonly ILogger<UnsubTgBotCommand> _logger =
        serviceProvider.GetRequiredService<ILogger<UnsubTgBotCommand>>();

    protected override string Command { get; } = "/unsub";
    protected override string Description { get; } = "unsubscribe from notifications";

    public override async Task ExecuteAsync(TelegramBotClient botClient, Message message, UpdateType updateType,
        CancellationToken token = default)
    {
        var username = message.Chat.Username ?? throw new ArgumentException(nameof(message.Chat));
        _logger.LogInformation("Try execute {command} for user {user}", GetType().Name, username);

        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider
            .GetRequiredService<
                ICommand<ChangeSubscriberActivityCommandContext, ChangeSubscriberActivityCommandResult>>();

        var context = new ChangeSubscriberActivityCommandContext(message.Chat.Id, false);
        var result = await command.ExecuteAsync(context, token);

        _logger.LogInformation("{command} result is {result}", GetType().Name, GetJson(result));

        await botClient.SendMessage(message.Chat.Id, "OK", ParseMode.MarkdownV2, cancellationToken: token);
    }
}