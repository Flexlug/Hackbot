using Hackbot.Services;
using Hackbot.Services.Implementations;
using Hackbot.Structures;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Главное меню, которое должен видеть капитан команды
    /// </summary>
    public class MainMenuCaptainScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Гильдия, капитаном которой является пользователь
        /// </summary>
        private Guild CurrentGuild { get; set; }

        /// <summary>
        /// Сервис, предоставляющий доступ к БД гильдий
        /// </summary>
        private IGuildsService guilds { get; set; }

        public MainMenuCaptainScene(long captianId)
        {
            ReplyKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Удалить команду", "delete_guild")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Управление командой", "edit_guild")
                }
            });

            guilds = GuildsService.GetInstance();

            CurrentGuild = guilds.GetGuildByCaptianAsync(captianId).Result;
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            switch (Stage)
            {
                case 0:
                    Member captain = CurrentGuild.Members.First(x => x.Id == CurrentGuild.CaptainId);


                    logger.Debug($"Reached stage 0. chatid: {ans?.Chat.Id}");
                    Stage++;
                    return Respond($"Главное меню капитана команды {CurrentGuild.Name}.");

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
                        case "delete_guild":
                            logger.Debug($"Reached case \"delete_guild\". Reequesting next scene. chatid: {ans.Chat.Id}");
                            Stage = 2;

                            return Respond($"Вы уверены? Введите название команды, чтобы подтвердить действие.\nНазвание команды: {CurrentGuild.Name}");

                        default:
                            logger.Debug($"Reached default case. chatid: {ans.Chat.Id}");
                            return Respond("Ответ не распознан.");
                    }
                case 2:
                    if (ans.Text == CurrentGuild.Name)
                    {
                        logger.Debug($"Requested deleting guild {CurrentGuild.Name}. Deleting...");
                        await guilds.RemoveGuildAsync(CurrentGuild);
                        logger.Debug($"Guild deleted");

                        Stage = 1;

                        return MainMenu("Команда удалена.");
                    }
                    else
                    {
                        Stage = 1;
                        return Respond("Удаление команды отменено.");
                    }

                default:
                    logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return Respond("Ответ не распознан. Возврат к главному меню.");
            }
        }
    }
}
