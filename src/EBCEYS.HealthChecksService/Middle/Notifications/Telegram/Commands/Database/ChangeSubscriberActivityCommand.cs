using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;

public record ChangeSubscriberActivityCommandContext(long ChatId, bool IsActive);

public record ChangeSubscriberActivityCommandResult(bool Result);

public class ChangeSubscriberActivityCommand(
    TelegramDbContext dbContext,
    ILogger<ChangeSubscriberActivityCommand> logger)
    : ICommand<ChangeSubscriberActivityCommandContext, ChangeSubscriberActivityCommandResult>
{
    public async Task<ChangeSubscriberActivityCommandResult> ExecuteAsync(
        ChangeSubscriberActivityCommandContext context, CancellationToken token)
    {
        logger.LogInformation("Executing {command} with context {context}", GetType().Name,
            $"id: {context.ChatId} active: {context.IsActive}");

        var sub = await dbContext.Subscribers.FirstOrDefaultAsync(s => s.ChatId == context.ChatId, token);

        if (sub == null || sub.IsActive == context.IsActive)
        {
            logger.LogInformation("Command {command} executed with result {result}", GetType().Name, false);
            return new ChangeSubscriberActivityCommandResult(false);
        }

        sub.IsActive = context.IsActive;

        var result = await dbContext.SaveChangesAsync(token) > 0;

        logger.LogInformation("Command {command} executed with result {result}", GetType().Name, result);

        return new ChangeSubscriberActivityCommandResult(result);
    }
}

public static class ChangeSubscriberActivityCommandExtensions
{
    public static IServiceCollection AddChangeSubscriberActivityCommand(this IServiceCollection services)
    {
        return services
            .AddScoped<ICommand<ChangeSubscriberActivityCommandContext, ChangeSubscriberActivityCommandResult>,
                ChangeSubscriberActivityCommand>();
    }
}