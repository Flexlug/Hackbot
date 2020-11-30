using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Hackbot.Services.Implementations
{
    /// <summary>
    /// Позволяет получить информацию о пользователе, зная только его ID
    /// </summary>
    public class UserGetterService : IUserGetterService
    {
        /// <summary>
        /// Клиент, через который бот взаимодействует с Telegram API
        /// </summary>
        private ITelegramBotClient client { get; set; }

        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(long userId)
        {
            try
            {
                return (await client.GetChatMemberAsync(userId, (int)userId))?.User;
            }
            catch(Exception ex)
            {
                logger.Error($"Could not find user {userId}.");
                return null;
            }
        }

        #region Singleton

        private static UserGetterService _instance = null;

        public static void Initialize(ITelegramBotClient client)
        {
            _instance = new UserGetterService(client);
        }

        public static UserGetterService GetInstance()
        {
            return _instance;
        }

        private UserGetterService(ITelegramBotClient client)
        {
            this.client = client;
        }

        #endregion
    }
}
