using Hackbot.Controllers;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services.Implementations
{
    public class SceneControllerNotifyer : ISceneControllerNotifyer
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public async Task RemoveUserDialogAsync(long userId)
        {
            logger.Debug($"Requested dialog delete for {userId}");
            await OnRemoveUserDialog.Invoke(userId);
        }

        public delegate Task RemoveUserDialogHanlder(long userId);
        /// <summary>
        /// Наступает при вызове удаления диалога пользователя
        /// </summary>
        public event RemoveUserDialogHanlder OnRemoveUserDialog;

        #region Singleton

        private static SceneControllerNotifyer _instance = null;

        public static SceneControllerNotifyer GetInstance()
        {
            if (_instance == null)
                _instance = new SceneControllerNotifyer();

            return _instance;
        }

        private SceneControllerNotifyer() {  }

        #endregion
    }
}
