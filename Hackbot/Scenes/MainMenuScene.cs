using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Hackbot.Services;
using Hackbot.Structures;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Hackbot.Services.Implementations;

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

        /// <summary>
        /// Разметка клавиатуры в главном меню
        /// </summary>
        private string[] keyboardMarkup = new string[] { "Зарегистрировать команду", "register_guild",
                                                         "Поиск команды", "search_guild" };


        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            switch (Stage)
            {
                case 0:
                    NextStage();
                    return Respond("Вы используете бот Hackbot.\n\nДанная разработка поможет вам найти подходящую для Вас команду. Также Вы можете создать свою. Для этого выберете соответствующий пункт.",
                                   GenerateKeyboard(keyboardMarkup));

                case 1:
                    if (CheckMenuEscape(ans))
                        return MainMenu();

                    if (ans.Text == "getmyid") 
                    {
                        return Respond($"Your id: {ans.From.Id}, Your phone: {ans.Contact.PhoneNumber}",
                                       GenerateKeyboard(keyboardMarkup));                     
                    }

                    switch (ans.InlineData)
                    {
                        case "register_guild":
                            logger.Debug($"Reached case \"зарегистрировать команду\". Reequesting next scene. chatid: {ans.Chat.Id}");
                            return NextScene(SceneTable.RegisterGuild);

                        case "search_guild":
                            logger.Debug($"Reached case \"поиск команды\". Requesting next scene chatid: {ans.Chat.Id}");
                            return NextScene(SceneTable.SearchGuild);

                        default:
                            logger.Debug($"Reached default case. chatid: {ans.Chat.Id}");
                            return Respond("Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                default:
                    return Respond("Ответ не распознан. Возврат к главному меню.",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }
    }
}
