using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(long userId) => (await client.GetChatMemberAsync(userId, (int)userId)).User;

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
