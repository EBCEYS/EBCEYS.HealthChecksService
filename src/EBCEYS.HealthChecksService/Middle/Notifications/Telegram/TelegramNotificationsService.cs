using EBCEYS.HealthChecksService.Docker.Extensions;
using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;
using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;
using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Methods;
using EBCEYS.HealthChecksService.Middle.Options;
using EBCEYS.HealthChecksService.SystemObjects;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Message = Telegram.Bot.Types.Message;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram;

public class TelegramNotificationsService(
    IServiceProvider serviceProvider,
    HealthChecksNotificationsQueue queue,
    ILogger<TelegramNotificationsService> logger) : BackgroundService
{
    private readonly TelegramBotConfiguration? _configuration = TelegramBotConfiguration.CreateFromEnvironment();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = _configuration != null
            ? new TelegramBotClient(_configuration.ApiKey,
                cancellationToken: stoppingToken)
            : null;

        if (bot != null)
        {
            logger.LogInformation("Starting Telegram notification service");
            var me = await bot.GetMe(stoppingToken);
            logger.LogInformation("Me info: {bot}", GetJson(me));
            bot.OnMessage += OnMessageReceived;
            bot.OnError += OnExceptionReceived;
        }

        await foreach (var notification in queue.GetNotesAsync(stoppingToken))
            try
            {
                logger.LogInformation("Get notification for container {note} with health entry: {entry}",
                    notification.ContainerList.GetName(), GetJson(notification.HealthReportEntry));
                var subsResult = await GetActiveSubsAsync(new GetActiveSubscribersCommandContext(), stoppingToken);
                foreach (var subs in subsResult.Subscribers.Chunk(5))
                {
                    var tasks = SendNotificationsAsync(bot, subs, notification, stoppingToken);
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending notifications");
            }

        return;

        Task OnExceptionReceived(Exception exception, HandleErrorSource source)
        {
            logger.LogError(exception, "Error on tg bot!");
            return Task.CompletedTask;
        }

        async Task OnMessageReceived(Message message, UpdateType updateType)
        {
            logger.LogDebug("Received from user {username} in chat {chatId} message: {message}", message.Chat.Username,
                message.Chat.Id, message.Text);
            try
            {
                var command = TelegramBotMethodBase.CreateFromMessage(message, serviceProvider);
                await command.ExecuteAsync(bot, message, updateType, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on message received!");
            }
        }
    }

    private static IEnumerable<Task> SendNotificationsAsync(TelegramBotClient? bot, TgBotSubscriber[] subs,
        TelegramNotificationInfo notification, CancellationToken stoppingToken)
    {
        if (bot == null) return [];
        return subs.Select(sub => bot.SendMessage(sub.ChatId, FormatNotification(notification), ParseMode.MarkdownV2,
            cancellationToken: stoppingToken));
    }

    private static string FormatNotification(TelegramNotificationInfo notification)
    {
        return
            $"Container {GetJsonForMessage(new { Name = notification.ContainerList.GetName(), notification.ContainerList.Image, notification.ContainerList.State, notification.ContainerList.Status })} with health entry: {GetJsonForMessage(notification.HealthReportEntry)}";
    }

    private async Task<GetActiveSubscribersCommandResult> GetActiveSubsAsync(GetActiveSubscribersCommandContext context,
        CancellationToken token)
    {
        return await ExecuteCommandAsync<GetActiveSubscribersCommandContext, GetActiveSubscribersCommandResult>(context,
            token);
    }

    private async Task<TCommandResult> ExecuteCommandAsync<TCommandContext, TCommandResult>(TCommandContext context,
        CancellationToken token) where TCommandContext : class where TCommandResult : class
    {
        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<ICommand<TCommandContext, TCommandResult>>();
        return await command.ExecuteAsync(context, token);
    }

    private static string GetJson<T>(T obj)
    {
        return TelegramBotMethodBase.GetJson(obj);
    }

    private static string GetJsonForMessage<T>(T obj)
    {
        return TelegramBotMethodBase.GetJsonForMessage(obj);
    }
}