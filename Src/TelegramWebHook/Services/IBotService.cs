using Telegram.Bot;

namespace TelegramWebHook.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}