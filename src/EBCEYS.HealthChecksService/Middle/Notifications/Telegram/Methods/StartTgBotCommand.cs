using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;

public class StartTgBotCommand(IServiceProvider serviceProvider) : TelegramBotMethodBase
{
    private readonly ILogger<StartTgBotCommand> _logger =
        serviceProvider.GetRequiredService<ILogger<StartTgBotCommand>>();

    protected override string Command { get; } = "/start";
    protected override string Description { get; } = "Use to subscribe on health checks notifications";

    public override async Task ExecuteAsync(TelegramBotClient botClient, Message message, UpdateType updateType,
        CancellationToken token = default)
    {
        var username = message.Chat.Username ?? throw new ArgumentException(nameof(message.Chat));
        _logger.LogInformation("Try execute {command} for user {user}", GetType().Name, username);
        var context = new CreateSubscriberCommandContext(username, message.Chat.Id);
        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider
            .GetRequiredService<ICommand<CreateSubscriberCommandContext, CreateSubscriberCommandResult>>();
        var commandResult = await command.ExecuteAsync(context, token);
        if (commandResult.Success)
            await botClient.SendMessage(message.Chat.Id, "Successfully subscribed", ParseMode.MarkdownV2,
                cancellationToken: token);
        _logger.LogInformation("{command} result is {result}", GetType().Name, GetJson(commandResult));
    }
}