using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Hackbot.Services;
using Hackbot.Structures;
using Hackbot.Services.Implementations;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using Centvrio.Emoji;

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
        private string[] keyboardMarkup = new string[] { $"{OtherSymbols.HeavyPlus} Зарегистрировать команду", "register_guild",
                                                         $"{Science.Telescope} Поиск команды", "search_guild" };


        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            switch (Stage)
            {
                case 0:
                    NextStage();
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Вы используете бот Hackbot. {OtherSymbols.WhiteHeavyCheckMark}\n\n{Alphanum.Information} Данная разработка поможет вам найти подходящую для Вас команду. Также Вы можете создать свою. Для этого выберете соответствующий пункт.",
                                   GenerateKeyboard(keyboardMarkup));

                case 1:
                    if (CheckMenuEscape(ans))
                        return MainMenu();

                    if (ans.Text == "getmyid") 
                    {
                        return Respond($"{Arrow.Right} Your id: {ans.From.Id}",
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
                            return Respond($"{OtherSymbols.Question} Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                default:
                    return Respond($"{OtherSymbols.Question} Ответ не распознан. \nВозврат к главному меню",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }
    }
}
