using Telegram.Bot.Types;

namespace EBCEYS.HealthChecksService.Middle.Notifications.Telegram.Exceptions;

public class TelegramBotMessageException : Exception
{
    public TelegramBotMessageException(Message tgMessage, string message) : base(message)
    {
        TgMessage = tgMessage;
    }

    public TelegramBotMessageException(string message) : base(message)
    {
    }

    public Message? TgMessage { get; }
}