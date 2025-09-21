using EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Database;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.HealthChecksService.SystemObjects;

public class BeforeStartService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelegramDbContext>();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any()) await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}