using System.Threading.Channels;
using Docker.DotNet.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram;

public class HealthChecksNotificationsQueue
{
    private readonly Channel<TelegramNotificationInfo> _channel = Channel.CreateUnbounded<TelegramNotificationInfo>();

    public async Task EnqueueAsync(TelegramNotificationInfo notification, CancellationToken token)
    {
        await _channel.Writer.WriteAsync(notification, token);
    }

    public async Task EnqueueAsync(ContainerListResponse container, HealthReportEntry healthReport,
        CancellationToken token)
    {
        await EnqueueAsync(new TelegramNotificationInfo(container, healthReport), token);
    }

    public IAsyncEnumerable<TelegramNotificationInfo> GetNotesAsync(CancellationToken token)
    {
        return _channel.Reader.ReadAllAsync(token);
    }
}