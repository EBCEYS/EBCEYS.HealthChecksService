using Docker.DotNet.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram;

public record TelegramNotificationInfo(ContainerListResponse ContainerList, HealthReportEntry HealthReportEntry);