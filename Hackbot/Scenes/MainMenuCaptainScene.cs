using Centvrio.Emoji;
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
        /// Гильдия, капитаном которой является пользователь
        /// </summary>
        private Guild CurrentGuild { get; set; }

        /// <summary>
        /// Сервис, предоставляющий доступ к БД гильдий
        /// </summary>
        private IGuildsService guilds { get; set; }

        private string[] keyboardMarkup = new string[] { $"{Tool.Wrench} Управление командой", "edit_guild" };

        public MainMenuCaptainScene(long captianId)
        {
            guilds = GuildsService.GetInstance();

            CurrentGuild = guilds.GetGuildByCaptianAsync(captianId).Result;
            Logger = LogManager.GetCurrentClassLogger();
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            switch (Stage)
            {
                case 0:
                    Member captain = CurrentGuild.Members.First(x => x.Id == CurrentGuild.CaptainId);

                    NextStage();
                    return Respond($"{Alphanum.Information} Главное меню капитана команды {CurrentGuild.Name}.",
                                   GenerateKeyboard(keyboardMarkup));

                case 1:

                    if (CheckMenuEscape(ans))
                        return MainMenu();

                    if (ans.Text == "getmyid")
                    {
                        Logger.Debug($"Requested \"getmyid\". Returning id: {ans.Chat.Id}");
                        return Respond($"Your id: {ans.From.Id}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    switch (ans.InlineData)
                    {
                        case "edit_guild":
                            return NextScene(SceneTable.CaptainGuildEditScene, CurrentGuild);

                        default:
                            return Respond($"{OtherSymbols.Question} Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                default:
                    Logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return Respond($"{OtherSymbols.Question} Ответ не распознан. Возврат к главному меню.",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }
    }
}
