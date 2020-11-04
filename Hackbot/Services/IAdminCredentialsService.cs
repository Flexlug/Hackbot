using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    public interface IAdminCredentialsService
    {
        /// <summary>
        /// Проверить, имеет ли пользователь особые привилегии
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <returns></returns>
        public Task<bool> CheckAsync(int userId);
    }
}
