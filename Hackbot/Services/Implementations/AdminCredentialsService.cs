using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;


using Hackbot.Services.DbContexts;
using Newtonsoft.Json.Bson;
using System.Threading.Tasks;
using Hackbot.Threading;

namespace Hackbot.Services.Implementations
{
    /// <summary>
    /// Сервис для проверки учётных данных в БД админов
    /// </summary>
    public class AdminCredentialsService : IAdminCredentialsService
    {
        private AdminCredentialsContext adminDB;
        private BackgroundQueue queue;

        public bool Check(int userId) => adminDB.Admins.FirstOrDefault(x => x.UserId == userId) != null ? true : false;

        public Task<bool> CheckAsync(int userId) => queue.QueueTask(() => Check(userId));

        #region Singleton

        private static AdminCredentialsService _instance = null;
        private AdminCredentialsService()
        {
            adminDB = new AdminCredentialsContext();
            queue = BackgroundQueue.GetInstance();
        }

        public static AdminCredentialsService GetInstance()
        {
            if (_instance == null)
                _instance = new AdminCredentialsService();

            return _instance;
        }

        #endregion
    }
}
