using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hackbot.Services;
using Hackbot.Structures;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NLog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Главное меню
    /// </summary>
    public class MainMenuScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        public MainMenuScene()
        {
            ReplyKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Зарегистрировать команду", "register_guild")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Поиск команды", "search_guild")
                    }
                });
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            switch (Stage)
            {
                case 0:
                    logger.Debug($"Reached stage 0. chatid: {ans?.Chat.Id}");
                    Stage++;
                    return Respond("Вы используете бот Hackbot.\n\nДанная разработка поможет вам найти подходящую для Вас команду. Также Вы можете создать свою. Для этого выберете соответствующий пункт.");

                case 1:
                    logger.Debug($"Reached stage 0. chatid: {ans.Chat.Id}");

                    if (CheckMenuEscape(ans))
                        return MainMenu();

                    if (ans.Text == "getmyid")
                    {
                        logger.Debug($"Requested \"getmyid\". Returning id: {ans.Chat.Id}");
                        return Respond($"Your id: {ans.From.Id}");
                    }

                    switch (ans.InlineData)
                    {
                        case "register_guild":
                            logger.Debug($"Reached case \"зарегистрировать команду\". Reequesting next scene. chatid: {ans.Chat.Id}");
                            return NextScene(SceneTable.RegisterGuild);

                        case "search_guild":
                            logger.Debug($"Reached case \"поиск команды\". Requesting next scene chatid: {ans.Chat.Id}");
                            return NextScene(SceneTable.SearchGuild);

                        case "getmyid":
                            logger.Debug($"Reached case \"getmyid\". Returning id: {ans.Chat.Id}");
                            return Respond($"Your id: {ans.From.Id}, chatid: {ans.Chat.Id}");

                        default:
                            logger.Debug($"Reached default case. chatid: {ans.Chat.Id}");
                            return Respond("Ответ не распознан.");                    
                    }

                default:
                    logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return Respond("Ответ не распознан. Возврат к главному меню.");
            }
        }
    }
}
