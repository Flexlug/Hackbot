using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;

namespace Hackbot.Services.Implementations
{
    /// <summary>
    /// Cервис, который позволит отправлять уведомления пользователям
    /// </summary>
    public class NotifyService : INotifyService
    {
        private ITelegramBotClient client;
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public async Task NotifyAsync(long userId, string text)
        {
            try
            {
                await client.SendTextMessageAsync(userId, text);
            }
            catch(Exception e)
            {
                logger.Error($"Could not find chat {userId}. Sending text: {text}");
            }
        }

        #region Singleton

        private static NotifyService _instance = null;
        private NotifyService(ITelegramBotClient client) 
        {
            this.client = client;
        }
        
        public static void Initialize(ITelegramBotClient client)
        {
            if (_instance == null)
                _instance = new NotifyService(client);
        }

        public static NotifyService GetInstance()
        {
            return _instance;
        }

        #endregion
    }
}
