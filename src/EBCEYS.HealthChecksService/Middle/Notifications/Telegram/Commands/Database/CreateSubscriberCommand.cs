using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;

public record CreateSubscriberCommandContext(string Subscriber, long ChatId);

public record CreateSubscriberCommandResult(bool Success);

public class CreateSubscriberCommand(TelegramDbContext dbContext, ILogger<CreateSubscriberCommand> logger)
    : ICommand<CreateSubscriberCommandContext, CreateSubscriberCommandResult>
{
    public async Task<CreateSubscriberCommandResult> ExecuteAsync(CreateSubscriberCommandContext context,
        CancellationToken token)
    {
        logger.LogInformation("Executing {command} with context {context}", GetType().Name, context.Subscriber);

        var existing = await dbContext.Subscribers.FirstOrDefaultAsync(s => s.ChatId == context.ChatId, token);

        if (existing != null)
        {
            existing.IsActive = true;
            var updateResult = await dbContext.SaveChangesAsync(token) > 0;

            logger.LogInformation("Command {command} executed with result {result}", GetType().Name, updateResult);
            return new CreateSubscriberCommandResult(updateResult);
        }

        var newUser = new TgBotSubscriber
        {
            ChatId = context.ChatId,
            Subscriber = context.Subscriber,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await dbContext.AddAsync(newUser, token);
        var result = await dbContext.SaveChangesAsync(token) > 0;

        logger.LogInformation("Command {command} executed with result {result}", GetType().Name, result);
        return new CreateSubscriberCommandResult(result);
    }
}

public static class CreateSubscriberCommandExtensions
{
    public static IServiceCollection AddCreateSubscriberCommand(this IServiceCollection services)
    {
        return services
            .AddScoped<ICommand<CreateSubscriberCommandContext, CreateSubscriberCommandResult>,
                CreateSubscriberCommand>();
    }
}