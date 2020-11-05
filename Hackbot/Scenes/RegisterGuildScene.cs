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
        /// Краткое резюме капитана
        /// </summary>
        private string captainDescrption = string.Empty;

        /// <summary>
        /// Имя капитана, которое узнаётся в процессе диалога с пользователем
        /// </summary>
        private string captainName = string.Empty;

        /// <summary>
        /// Название команды, которое узнаётся в процессе диалога с пользователем
        /// </summary>
        private string guildName = string.Empty;

        /// <summary>
        /// Описание команды. Может включать в себя информацию как об участниках, так и о примерном направлении работы. И с какими кейсами они будут работать.
        /// </summary>
        private string guildDescription = string.Empty;

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
                // Начало регистрации
                // Ввод имени
                case 0:
                    Stage++;
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");
                    return Respond("Запущен процесс регистрации команды. Вам нужно будет заполнить информацию о себе и новой команде.\n\nНачнём с Вас.\nВведите пожалуйста своё имя.");

                // Проверка имени на валидность
                // Ввод Member.Description
                case 1:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");
                    if (!string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Name valid {ans.Text}");
                        Stage++;
                        captainName = ans.Text;
                        return Respond("Опишите в 2-3 предложениях свои основные навыки и чем вы можете быть полезны команде.");
                    }
                    else
                    {
                        logger.Debug($"Name is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое имя. Повторите, пожалуйста, ещё раз.");
                    }

                // Проверка Member.Description на валидность
                // Вопрос о правильности введённых данных
                case 2:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Description is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое сообщение. Не стесняйтесь написать хоть что-нибудь о себе. В противном же случае просто напишите \"капитан\" =).");
                    }

                    logger.Debug($"Description valid {ans.Text}");
                    captainDescrption = ans.Text;

                    Stage++;
                    ReplyKeyboard = YesNoReplyKeyboard;
                    return Respond($"Ваше имя: {captainName}\nВаши навыки: {captainDescrption}\n\n\nПодтвердите правильность введённых данных.");

                // Yes/No на вопрос о правильности составленного "резюме"
                // Выбор названия команды
                case 3:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (ans.InlineData == "no")
                    {
                        logger.Debug($"Inline NO. Returning to stage 1");

                        ReplyKeyboard = StandardReplyKeyboard;
                        Stage = 1;
                        return Respond("Процесс ввода ваших данных запущен заново. Введите пожалуйста своё имя.");
                    }

                    if (ans.InlineData != "yes")
                    {
                        logger.Debug($"Invalid inline. Retrying...");
                        ReplyKeyboard = YesNoReplyKeyboard;
                        return Respond($"Ответ не распознан.\n\nВаше имя: {captainName}\nВаши навыки: {captainDescrption}\n\n\nПодтвердите правильность введённых данных.");
                    }

                    Stage++;
                    ReplyKeyboard = StandardReplyKeyboard;
                    return Respond("Теперь про команду.\n\nВведите название команды.");

                // Проверка названия команды на валидность
                // Выбор описания команды
                case 4:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    // Проверка на валидность введенного названия команды
                    if (string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Name is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое название команды. Повторите ещё раз.");
                    }

                    logger.Debug($"Guild name valid {ans.Text}");
                    guildName = ans.Text;

                    Stage++;
                    ReplyKeyboard = StandardReplyKeyboard;
                    return Respond($"Вы ввели название команды: {ans.Text}\nТеперь напишите краткое описание команды, чтобы Вашим будущим сокомандникам было понятно, в каком направлении преимущественно будет вестись работа. Можете указать конкретные кейсы, которым скорее всего будет отдано предпочтение.");

                // Проверка описания команды на валидность
                // Выбор роли
                case 5:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    // Проверка на валидность введенного названия команды
                    if (string.IsNullOrWhiteSpace(ans.Text.ToLower()))
                    {
                        logger.Debug($"Name is null or whitespace. chatid: {ans.Chat.Id}.");
                        return Respond("Вы ввели пустое описание команды. Повторите ещё раз.");
                    }

                    logger.Debug($"Guild name valid {ans.Text}");
                    guildDescription = ans.Text;

                    Stage++;
                    ReplyKeyboard = ChooseRoleReplyKeyboard;
                    return Respond($"Вы ввели описание команды: {ans.Text}\nВыберете пожалуйста, какую роль вы будете выполнять в команде (помимо капитанской).");

                // Проверка введенной роли на валидность
                // Подтверждение введённых данных о команде
                case 6:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    GuildRoles? role = Converter.FromStrToGuildRole(ans.InlineData);
                    if (role == null)
                    {
                        logger.Debug($"Converter couldn't convert role. input: {ans.InlineData}");
                        return Respond("Ошибка при распознавании роли. Повторите, пожалуйста, выбор роли.");
                    }

                    logger.Debug("Role valid");
                    captainRole = (GuildRoles)role;

                    Stage++;
                    ReplyKeyboard = YesNoReplyKeyboard;
                    return Respond($"Название команды: {guildName}\nОписание команды: {guildDescription}\nВаша роль: {Converter.GuildRoleToStr(captainRole)}\n\nПодтвердите правильность введённых данных.");

                // Подтверждение введённых данных о команде. Создание команды
                case 7:
                    logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (ans.InlineData == "no")
                    {
                        Stage = 3;
                        ReplyKeyboard = StandardReplyKeyboard;
                        return Respond("Процесс регистрации команды запущен заново. Введите пожалуйста название команды.");
                    }

                    if (ans.InlineData != "yes")
                    {
                        ReplyKeyboard = YesNoReplyKeyboard;
                        return Respond($"Ответ не распознан.\n\nВаше имя: {captainName}\nНазвание команды: {guildName}\nВаша роль: {Converter.GuildRoleToStr(captainRole)}\n\nПодтвердите правильность введённых данных.");
                    }

                    logger.Debug($"Creating guild");
                    Guild guild = new Guild()
                    {
                        CaptainId = ans.From.Id,

                        Name = guildName,
                        Description = guildDescription,
                        InSearching = true,
                        Members = new List<Member>()
                        {
                            new Member()
                            {
                                Id = ans.From.Id,

                                Name = captainName,
                                Description = captainDescrption,
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
