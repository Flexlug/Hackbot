using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    /// <summary>
    /// Интерфейс, описывающий сервис для проверки учётных данных в БД админов
    /// </summary>
    public interface IAdminCredentialsService
    {
        /// <summary>
        /// Проверить, имеет ли пользователь особые привилегии
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public Task<bool> CheckAsync(int userId);

        /// <summary>
        /// Проверить, имеет ли пользователь особые привилегии
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public bool Check(int userId);
    }
}
