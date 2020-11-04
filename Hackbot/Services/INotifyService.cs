using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    /// <summary>
    /// Интерфейс, описывающий сервис, который позволит отправлять уведомления пользователям
    /// </summary>
    public interface INotifyService
    {
        /// <summary>
        /// Отправить пользователю текстовое уведомление
        /// </summary>
        /// <param name="userId">ID получателя</param>
        /// <param name="text">Текст сообщения</param>
        public Task NotifyAsync(long userId, string text);
    }
}
