using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using System.Text;

using Hackbot.Scenes;

using Telegram.Bot.Types;
using Hackbot.Structures;
using NLog;
using Hackbot.Services;
using Hackbot.Services.Implementations;
using NLog.Fluent;

namespace Hackbot.Controllers
{
    /// <summary>
    /// Получает сообщения и назначает нужный сценарий диалога
    /// </summary>
    public class SceneController
    {
        /// <summary>
        /// Словарь со всеми диалогами
        /// </summary>
        private Dictionary<long, Scene> scenes;

        /// <summary>
        /// Таблица со всеми сценами
        /// </summary>
        private Dictionary<SceneTable, Type> sceneTable;

        /// <summary>
        /// Логер для этого класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Сервис для доступа к БД с гильдиями. Для проверки пользователя, является ли тот капитаном.
        /// </summary>
        private IGuildsService guildsService;

        public SceneController() 
        {
            scenes = new Dictionary<long, Scene>();

            SceneControllerNotifyer.GetInstance().OnRemoveUserDialog += SceneController_OnRemoveUserDialog;

            sceneTable = new Dictionary<SceneTable, Type>()
            {
                { SceneTable.MainMenu, typeof(MainMenuScene) },
                { SceneTable.MainAdminMenu, typeof(MainMenuAdminScene) },
                { SceneTable.RegisterGuild, typeof(RegisterGuildScene) },
                { SceneTable.MainCaptainMenu, typeof(MainMenuCaptainScene) },
                { SceneTable.MainMemberMenu, typeof(MainMenuMemberScene) },
                { SceneTable.CaptainGuildEditScene, typeof(CaptainGuildEditScene) },
                { SceneTable.SearchGuild, typeof(SearchGuildScene) },
                { SceneTable.CaptainRequestsView, typeof(CaptainRequestsViewScene) }
            };

            guildsService = GuildsService.GetInstance();

            logger.Debug("Avaliable scenes:");
            foreach (var item in sceneTable)
            {
                logger.Debug($"{item.Key}");
            }
        }

        /// <summary>
        /// Выполняется при исключении членов команды или при удалении команды. Необходимо для сброса диалога
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task SceneController_OnRemoveUserDialog(long userId)
        {
            if (scenes.ContainsKey(userId))
            {
                logger.Debug($"Removed scene {userId}");
                scenes.Remove(userId);
            }

            logger.Warn($"Requested remove scene {userId}, but no such exists!");
        }

        /// <summary>
        /// Сгенерировать различные главные меню в зависимости от того, кем является пользователь
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureAsync()
        {
        //    List<long> members = await guildsService.GetAllMembersAsync();
        //    List<long> captains = await guildsService.GetAllCaptainsAsync();

        //    foreach (var cap in captains)
        //        members.Remove(cap);

        //    logger.Debug("Configuring main menus");

        //    foreach (var mem in members)
        //    {
        //        logger.Debug($"Member: {mem}");
        //        scenes.Add(mem, await GenerateSceneAsync(SceneTable.MainMemberMenu));
        //    }

        //    foreach (var cap in captains)
        //    {
        //        logger.Debug($"Captain: {cap}");
        //        scenes.Add(cap, await GenerateSceneAsync(SceneTable.MainCaptainMenu));
        //    }
        }

        /// <summary>
        /// Возвращает экземпляр сцены
        /// </summary>
        /// <param name="sceneName">Название сцены</param>
        /// <returns></returns>
        private async Task<Scene> GenerateSceneAsync(SceneTable sceneName, params object[] args)
        {
            if (sceneTable.ContainsKey(sceneName))
            {
                if (args != null && args.Length != 0)
                    return Activator.CreateInstance(sceneTable[sceneName], args) as Scene;
                else
                    return Activator.CreateInstance(sceneTable[sceneName]) as Scene;
            }
            else
            {
                logger.Error($"Could not find scene: {sceneName}. Returned to main menu.");
                return null;
            }
        }

        /// <summary>
        /// Возвращает различные вариации главного меню в зависимости от того, кем пользователь является: Новичком, Командиром, Участником
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        private async Task<Scene> GenerateMainMenuAsync(long chatId)
        {
            if (await guildsService.CheckCaptainAsync(chatId))
            {
                return await GenerateSceneAsync(SceneTable.MainCaptainMenu, chatId);
            }
            else
            {
                if (await guildsService.CheckMemberAsync(chatId))
                {
                    return await GenerateSceneAsync(SceneTable.MainMemberMenu, chatId);
                }
                else
                {
                    return await GenerateSceneAsync(SceneTable.MainMenu);
                }
            }
        }

        /// <summary>
        /// Сгенерировать ответ на сообщение пользователя
        /// </summary>
        /// <param name="ans">Сообщение пользователя</param>
        /// <returns></returns>
        public async Task<SceneResult> GenerateResult(RecievedMessage ans)
        {
            Scene currScene = null;

            // создаём новый диалог при отсутствии такового
            if (!scenes.ContainsKey(ans.Chat.Id) || scenes[ans.Chat.Id] == null)
            {
                currScene = await GenerateMainMenuAsync(ans.Chat.Id);

                scenes.Add(ans.Chat.Id, currScene);

                logger.Debug($"Added new dialog for {ans.From.Id}: {currScene}");
            }
            else
                currScene = scenes[ans.Chat.Id];

            SceneResult res = null;

            if (ans.Text.ToLower() == "administrator")
                res = new SceneResult() {
                    SceneNextAction = SceneResult.SceneAction.Admin
                };
            else
                res = await currScene.GetResult(ans);

            // Переход на админ панель
            if (res.SceneNextAction == SceneResult.SceneAction.Admin)
            {
                logger.Warn("Requested administrator panel");
                currScene = await GenerateSceneAsync(SceneTable.MainAdminMenu) ?? await GenerateMainMenuAsync(ans.Chat.Id);
                res = await currScene.GetResult(ans);
            }

            // Переход на новую сцену
            if (res.SceneNextAction == SceneResult.SceneAction.Next)
            {
                logger.Info("Requested next scene");
                currScene = await GenerateSceneAsync(res.NextScene, res.NextSceneParams) ?? await GenerateMainMenuAsync(ans.Chat.Id);
                res = await currScene.GetResult(ans);
            }

            // Переход в главное меню
            if (res.SceneNextAction == SceneResult.SceneAction.Zeroize)
            {
                logger.Info("Requested zeroize");
                currScene = await GenerateMainMenuAsync(ans.Chat.Id);

                SceneResult newRes = await currScene.GetResult(ans);
                res.KeyboardMarkup = newRes.KeyboardMarkup;
            }

            scenes[ans.Chat.Id] = currScene;

            return res;
        }
    }
}
