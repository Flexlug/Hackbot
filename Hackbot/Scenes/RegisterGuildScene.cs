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
        /// Предоставляет доступ к БД с гильдями
        /// </summary>
        private IGuildsService guilds;

        public RegisterGuildScene()
        {
            guilds = GuildsService.GetInstance();

            Logger = LogManager.GetCurrentClassLogger();
        }

        // user input
        private string captainDescrption = string.Empty;
        private string captainName = string.Empty;
        private string guildName = string.Empty;
        private string guildDescription = string.Empty;
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
                    NextStage();
                    return Respond("Запущен процесс регистрации команды. Вам нужно будет заполнить информацию о себе и новой команде.\n\nНачнём с Вас.\nВведите пожалуйста своё имя.",
                                   GetStandardKeyboard());

                // Проверка имени на валидность
                // Ввод Member.Description
                case 1:
                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое имя. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    captainName = ans.Text;

                    NextStage();
                    return Respond("Опишите в 2-3 предложениях свои основные навыки и чем вы можете быть полезны команде.",
                                   GetStandardKeyboard());

                // Проверка Member.Description на валидность
                // Вопрос о правильности введённых данных
                case 2:
                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое сообщение. Не стесняйтесь написать хоть что-нибудь о себе. В противном же случае просто напишите \"капитан\" =).",
                                       GetStandardKeyboard());

                    captainDescrption = ans.Text;

                    NextStage();
                    return Respond($"Ваше имя: {captainName}\nВаши навыки: {captainDescrption}\n\n\nПодтвердите правильность введённых данных.",
                                   GetYesNoKeyboard());

                // Yes/No на вопрос о правильности составленного "резюме"
                // Выбор названия команды
                case 3:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond("Процесс ввода ваших данных запущен заново. Введите пожалуйста своё имя.",
                                       GetStandardKeyboard());
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"Ответ не распознан.\n\nВаше имя: {captainName}\nВаши навыки: {captainDescrption}\n\n\nПодтвердите правильность введённых данных.",
                                       GetYesNoKeyboard());

                    NextStage();
                    return Respond("Теперь про команду.\n\nВведите название команды.",
                                   GetStandardKeyboard());

                // Проверка названия команды на валидность
                // Выбор описания команды
                case 4:

                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое название команды. Повторите ещё раз.",
                                       GetStandardKeyboard());

                    guildName = ans.Text;

                    NextStage();
                    return Respond($"Вы ввели название команды: {ans.Text}\nТеперь напишите краткое описание команды, чтобы Вашим будущим сокомандникам было понятно, в каком направлении преимущественно будет вестись работа. Можете указать конкретные кейсы, которым скорее всего будет отдано предпочтение.",
                                   GetStandardKeyboard());

                // Проверка описания команды на валидность
                // Выбор роли
                case 5:

                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое описание команды. Повторите ещё раз.",
                                       GetStandardKeyboard());

                    guildDescription = ans.Text;

                    NextStage();
                    return Respond($"Вы ввели описание команды: {ans.Text}\nВыберете пожалуйста, какую роль вы будете выполнять в команде (помимо капитанской).",
                                   GetRolesKeyboard());

                // Проверка введенной роли на валидность
                // Подтверждение введённых данных о команде
                case 6:

                    GuildRoles? role = Converter.FromStrToGuildRole(ans.InlineData);
                    if (role == null)
                    {
                        Logger.Debug($"Converter couldn't convert role. input: {ans.InlineData}");
                        return Respond("Ошибка при распознавании роли. Повторите, пожалуйста, выбор роли.",
                                       GetRolesKeyboard());
                    }

                    captainRole = (GuildRoles)role;

                    NextStage();
                    return Respond($"Название команды: {guildName}\nОписание команды: {guildDescription}\nВаша роль: {Converter.GuildRoleToStr(captainRole)}\n\nПодтвердите правильность введённых данных.",
                                   GetYesNoKeyboard());

                // Подтверждение введённых данных о команде. Создание команды
                case 7:
                    Logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (CheckNegativeInline(ans))
                    {
                        ToStage(3);
                        return Respond("Процесс регистрации команды запущен заново.Введите пожалуйста название команды.",
                                       GetStandardKeyboard());
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"Ответ не распознан.\n\nВаше имя: {captainName}\nНазвание команды: {guildName}\nВаша роль: {Converter.GuildRoleToStr(captainRole)}\n\nПодтвердите правильность введённых данных.",
                                       GetYesNoKeyboard());


                    Logger.Debug($"Creating guild");
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

                    Logger.Debug($"Adding guild to DB.");
                    await guilds.AddGuildAsync(guild);

                    return MainMenu($"Команда \"{guild.Name}\" успешно создана.");

                default:
                    return Respond("Ответ не распознан.",
                                   GetStandardKeyboard());
            }
        }
    }
}
