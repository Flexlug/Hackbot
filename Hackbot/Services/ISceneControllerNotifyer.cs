using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    /// <summary>
    /// Интефрейс, описывающий сервис, который предназначен для сбрасывания управления у какого-либо пользователя
    /// </summary>
    public interface ISceneControllerNotifyer
    {
        /// <summary>
        /// Удаляет диалог с пользователем, тем самым заставляя программу заново проверить его привелегии
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public Task RemoveUserDialogAsync(long userId);
    }
}
