using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using Hackbot.Util;
using Hackbot.Services;
using Hackbot.Structures;
using Hackbot.Services.Implementations;

namespace Hackbot.Scenes
{
    class RegisterGuildScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Предоставляет доступ к БД с гильдями
        /// </summary>
        private IGuildsService guilds;

        /// <summary>
        /// Стаднартная разметка клавиатуры
        /// </summary>
        private InlineKeyboardMarkup StandardReplyKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню", "mainmenu")
                }
            });

        private InlineKeyboardMarkup YesNoReplyKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Да", "yes")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Нет", "no")
                }
            });

        private InlineKeyboardMarkup ChooseRoleReplyKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Backend разработчик", "role1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Frontend разработчик", "role2"), 
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ГИС специалист", "role3") 
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Дизайнер", "role4") 
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Менеджер", "role5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад в меню", "mainmenu")
                }
            });

        public RegisterGuildScene()
        {
            guilds = GuildsService.GetInstance();

            ReplyKeyboard = StandardReplyKeyboard;
        }

        /// <summary>
        /// Имя капитана, которое узнаётся в процессе диалога с пользователем
        /// </summary>
        private string captainName = string.Empty;

        /// <summary>
        /// Название команды, которое узнаётся в процессе диалога с пользователем
        /// </summary>
        private string guildName = string.Empty;

        /// <summary>
        /// Роль командира
        /// </summary>
        private GuildRoles captainRole;

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
                return MainMenu();

            // Обработка ответа
            switch (Stage)
            {
                case 0:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");
                    Stage++;
                    return Respond("Запущен процесс регистрации команды.\n\nВведите пожалуйста своё имя.");

                case 1:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");
                    if (!string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Name valid {ans.Text}");
                        Stage++;
                        captainName = ans.Text;
                        return Respond("Введите пожалуйста название новой команды.");
                    }
                    else
                    {
                        logger.Debug($"Name is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое имя.");
                    }

                case 2:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    // Проверка на валидность введенного названия команды
                    if (!string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Guild name valid {ans.Text}");
                        Stage++;

                        ReplyKeyboard = ChooseRoleReplyKeyboard;
                        guildName = ans.Text;
                        return Respond($"Вы ввели название команды: {ans.Text}\nВыберете пожалуйста, какую роль вы будете выполнять в команде (помимо капитанской).");
                    }
                    else
                    {
                        logger.Debug($"Name is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое название команды.");
                    }

                case 3:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");
                    GuildRoles? role = Converter.FromStrToGuildRole(ans.InlineData);

                    if (role == null)
                    {
                        logger.Debug($"Converter couldn't convert role. input: {ans.InlineData}");
                        return Respond("Повторите, пожалуйста, выбор роли.");
                    }

                    captainRole = (GuildRoles)role;
                    Stage++;
                    ReplyKeyboard = YesNoReplyKeyboard;
                    return Respond($"Ваше имя: {captainName}\nНазвание команды: {guildName}\nВаша роль: {Converter.GuildRoleToStr(captainRole)}\n\nПодтвердите правильность введённых данных.");


                case 4:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (ans.InlineData == "no")
                    {
                        Stage = 1;
                        ReplyKeyboard = StandardReplyKeyboard;
                        return Respond("Процесс регистрации команды запущен заново. Введите пожалуйста название команды.");
                    }

                    logger.Debug($"Creating guild");
                    Guild guild = new Guild()
                    {
                        CaptainId = ans.From.Id,
                        Name = guildName,
                        Members = new List<Member>()
                            {
                                new Member()
                                {
                                    Id = ans.From.Id,
                                    Name = captainName,
                                    Role = captainRole
                                }
                            },
                    };

                    logger.Debug($"Adding guild to DB.");
                    await guilds.AddGuildAsync(guild);

                    return MainMenu($"Команда \"{guild.Name}\" успешно создана.");

                default:
                    return Respond("Ответ не распознан.");
            }
        }
    }
}
