using Microsoft.Extensions.Options;
using MihaZupan;
using Telegram.Bot;
using TelegramWebHook;

namespace TelegramWebHook.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> config)
        {
            var socks5Host = "";
            var socks5Port = 0;
            var botToken = "<Token>";
            // use proxy if configured in appsettings.*.json
            Client = string.IsNullOrEmpty(socks5Host)
                ? new TelegramBotClient(botToken)
                : new TelegramBotClient(botToken, new HttpToSocks5Proxy(socks5Host, socks5Port));
        }

        public TelegramBotClient Client { get; }
    }
}
