using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Hackbot.Scenes;
using Hackbot.Services;
using Hackbot.Services.DbContexts;
using Hackbot.Services.Implementations;
using Hackbot.Structures;

using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using NLog;
using System.IO;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Главное меню, которое доступно только администраторам
    /// </summary>
    public class MainMenuAdminScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Сервис для проверки наличия пользователя в БД администраторов
        /// </summary>
        IAdminCredentialsService credentials;

        IGuildsService guilds;

        private string[] keyboardMarkup = new string[] { "CSV", "csv",
                                                         "Меню", "mainmenu" };
        
        public MainMenuAdminScene()
        {
            credentials = AdminCredentialsService.GetInstance();
            guilds = GuildsService.GetInstance();

            Logger = LogManager.GetCurrentClassLogger();
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
                return MainMenu();


            switch (Stage)
            {
                case 0:
                    // проверим наличие id пользователя в БД админов
                    if (!await credentials.CheckAsync(ans.From.Id))
                    {
                        logger.Warn($"Login as admin attempt: {ans.From.Id} refused");
                        return MainMenu("Доступ запрещен");
                    }

                    if (ans.Text == "administrator")
                    {
                        logger.Warn($"Login as admin attempt: {ans.From.Id} granted");
                        ToStage(1);

                        return Respond("Доступ разрешен",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    return MainMenu("Ответ не распознан");

                case 1:

                    switch (ans.InlineData)
                    {
                        case "csv":
                            List<Guild> guildList = await guilds.GetAllGuildsAsync();

                            // ИМЯ, РЕЗЮМЕ, РОЛЬ, TG_ID, КОМАНДА, КАПИТАН(да/нет)
                            StringBuilder sw = new StringBuilder();
                            sw.Append("csv format: ИМЯ, РЕЗЮМЕ, РОЛЬ, TG_ID, КОМАНДА, КАПИТАН(да/нет)\n");
                            foreach (Guild g in guildList)
                                foreach (Member mem in g.Members)
                                    sw.AppendLine($"{mem.Name}|{mem.Description}|{mem.Role}|{mem.Id}|{g.Name}|{(mem.Id == g.CaptainId ? "true" : "false")}");

                            return Respond(sw.ToString(),
                                           GenerateKeyboard(keyboardMarkup));

                        default:
                            return Respond("Ответ не распознан",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                default:
                    return Respond("Ответ не распознан",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }
    }
}
