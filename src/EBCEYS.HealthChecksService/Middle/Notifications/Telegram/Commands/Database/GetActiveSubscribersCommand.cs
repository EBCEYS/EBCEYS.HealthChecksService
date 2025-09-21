using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;
using EBCEYS.HealthChecksService.SystemObjects;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Commands.Database;

public record GetActiveSubscribersCommandContext;

public record GetActiveSubscribersCommandResult(IReadOnlyCollection<TgBotSubscriber> Subscribers);

public class GetActiveSubscribersCommand(TelegramDbContext dbContext, ILogger<GetActiveSubscribersCommand> logger)
    : ICommand<GetActiveSubscribersCommandContext, GetActiveSubscribersCommandResult>
{
    public async Task<GetActiveSubscribersCommandResult> ExecuteAsync(GetActiveSubscribersCommandContext context,
        CancellationToken token)
    {
        logger.LogInformation("Executing {command} with context {context}", GetType().Name, "none");

        var subs = await dbContext.Subscribers.AsNoTracking().Where(s => s.IsActive).ToArrayAsync(token);

        logger.LogInformation("Command {command} executed with result {result}", GetType().Name,
            string.Join(',', subs.Select(s => s.Subscriber).ToArray()));

        return new GetActiveSubscribersCommandResult(subs);
    }
}

public static class GetActiveSubscribersCommandExtensions
{
    public static IServiceCollection AddGetActiveSubscribersCommand(this IServiceCollection services)
    {
        return services
            .AddScoped<ICommand<GetActiveSubscribersCommandContext, GetActiveSubscribersCommandResult>,
                GetActiveSubscribersCommand>();
    }
}