using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;

public class GetAllSubsTgBotCommand(IServiceProvider serviceProvider) : TelegramBotMethodBase
{
    private readonly ILogger<GetAllSubsTgBotCommand> _logger =
        serviceProvider.GetRequiredService<ILogger<GetAllSubsTgBotCommand>>();

    protected override string Command { get; } = "/getallsubs";
    protected override string Description { get; } = "gets all subscribers";

    public override async Task ExecuteAsync(TelegramBotClient botClient, Message message, UpdateType updateType,
        CancellationToken token = default)
    {
        var username = message.Chat.Username ?? throw new ArgumentException(nameof(message.Chat));
        _logger.LogInformation("Try execute {command} for user {user}", GetType().Name, username);

        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider
            .GetRequiredService<ICommand<GetActiveSubscribersCommandContext, GetActiveSubscribersCommandResult>>();

        var context = new GetActiveSubscribersCommandContext();
        var result = await command.ExecuteAsync(context, token);

        _logger.LogInformation("{command} result is {result}", GetType().Name, GetJson(result));

        await botClient.SendMessage(message.Chat.Id, GetJsonForMessage(result.Subscribers), ParseMode.MarkdownV2,
            cancellationToken: token);
    }
}