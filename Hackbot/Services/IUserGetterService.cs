using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot.Types;

namespace Hackbot.Services
{
    /// <summary>
    /// Сервис для получения данных о контакте через ID
    /// </summary>
    public interface IUserGetterService
    {
        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<User> GetUserAsync(long userId);
    }
}
