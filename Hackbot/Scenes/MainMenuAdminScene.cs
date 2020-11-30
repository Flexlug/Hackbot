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
using System.Security.Cryptography;

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
        IDocumentSenderService sender;

        private string[] keyboardMarkup = new string[] { "CSV", "csv",
                                                         "Меню", "mainmenu" };
        
        public MainMenuAdminScene()
        {
            credentials = AdminCredentialsService.GetInstance();
            guilds = GuildsService.GetInstance();
            sender = DocumentSenderService.GetInstance();

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

                            try
                            {
                                // ИМЯ, РЕЗЮМЕ, РОЛЬ, TG_ID, КОМАНДА, КАПИТАН(да/нет)
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                                    {
                                        sw.WriteLine("name|description|role|TG_ID|team|is_captain");

                                        foreach (Guild g in guildList)
                                            foreach (Member mem in g.Members)
                                                sw.WriteLine($"{mem.Name}|{mem.Description}|{mem.Role}|{mem.Id}|{g.Name}|{(mem.Id == g.CaptainId ? "true" : "false")}");

                                        sw.Flush();
                                        ms.Seek(0, SeekOrigin.Begin);

                                        using (StreamReader sr = new StreamReader(ms, Encoding.UTF8, true))
                                        {
                                            sender.SendDocument(ans.From.Id, sr.BaseStream, $"{DateTime.Now.ToShortDateString()}-{DateTime.Now.Ticks}.csv");
                                        }
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                logger.Error($"Couldn't construct file. {e}");
                            }

                            return Respond("File construction complete",
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
