using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using Hackbot.Structures;

namespace Hackbot.Scenes
{
    class RegisterGuildScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        public RegisterGuildScene()
        {
            ReplyKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню", "mainmenu")
                }
            });
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            // Обработка ответа
            switch (Stage)
            {
                case 0:
                    logger.Debug($"Reached stage 0. chatid: {ans.Chat.Id}");
                    Stage++;
                    return Respond("Запущен процесс регистрации команды.\n\nВведите пожалуйста название команды");

                case 1:
                    logger.Debug($"Reached stage 1. chatid: {ans.Chat.Id}");

                    if (!string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Got guild name: {ans.Text}. Requested next stage. chatid: {ans.Chat.Id}");
                        return MainMenu($"Вы ввели название команды: {ans.Text}");
                    }
                    else
                    {
                        logger.Debug($"Invalid guild name. chatid: {ans.Chat.Id}");
                        return MainMenu("Вы ввели пустое название команды");
                    }

                default:
                    return Respond("Ответ не распознан.");
            }
        }
    }
}
