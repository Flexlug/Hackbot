using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Hackbot.Structures;
using Hackbot.Services;
using Telegram.Bot.Types.ReplyMarkups;
using Hackbot.Util;
using System.Reflection.Metadata.Ecma335;
using Hackbot.Services.Implementations;

namespace Hackbot.Scenes
{
    // TODO Реализовать сцену поиска команд
    /// <summary>
    /// Сцена для поиска команд
    /// </summary>
    public class SearchGuildScene : Scene
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
        /// Позволяет получать более подробные данные из ID пользователя. Необходимо для возвращения контактов капитана.
        /// </summary>
        private IUserGetterService users;

        /// <summary>
        /// Позволяет обрабатывать заявки на вступление
        /// </summary>
        private IRequestsService reqs;

        /// <summary>
        /// Позволяет отсылать уведомления командам
        /// </summary>
        private INotifyService notify;

        /// <summary>
        /// Возвращает текстовую информацию о команде
        /// </summary>
        /// <param name="guild">Ссылка на команду</param>
        /// <returns></returns>
        private async Task<string> PrintGuildInfo(Guild guild)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Название команды: {guild.Name}\n");
            sb.Append($"Описание: {guild.Description}\n");
            sb.Append($"Капитан: {await users.GetUserAsync(guild.CaptainId)}\n");
            //sb.Append($"Капитан: [ссылка](tg://user?id={guild.CaptainId})\n");
            sb.Append($"Количество участников: {guild.Members.Count}.\n");

            return sb.ToString();
        }

        private string[] guildNavKeyboardMarkup = new string[] { "Предыдущая команда", "prev_guild",
                                                                 "Следующая команда", "next_guild",
                                                                 "Отправить заявку", "request",
                                                                 "Меню", "mainmenu" };

        private string memberName = string.Empty;
        private string memberDescription = string.Empty;
        private GuildRoles memberRole;

        private List<Guild> avaliableGuilds = null;
        private int guildIter = 0;

        public SearchGuildScene()
        {
            Logger = LogManager.GetCurrentClassLogger();

            guilds = GuildsService.GetInstance();
            users = UserGetterService.GetInstance();
            reqs = RequestsService.GetInstance();
            notify = NotifyService.GetInstance();
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
                return MainMenu();

            switch(Stage)
            {
                // Ввод имени
                case 0:

                    NextStage();
                    return Respond("Запущен процесс поиска команды. Вам нужно будет заполнить информацию о себе.\n\nВведите пожалуйста своё имя.",
                                   GetStandardKeyboard());

                // Проверка имени на валидность
                // Ввод резюме
                case 1:
                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое имя. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    memberName = ans.Text;

                    NextStage();
                    return Respond("Опишите в 2-3 предложениях свои основные навыки и чем вы можете быть полезны команде.",
                                   GetStandardKeyboard());

                // Проверка резюме на валидность
                // Ввод роли
                case 2:
                    if (CheckEmptyMsgText(ans))
                        return Respond("Вы ввели пустое описание. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    memberDescription = ans.Text;

                    NextStage();
                    return Respond("Выберете роль, на которую вы претендуете.",
                                   GetRolesKeyboard());

                // Проверка ввода роли
                // Поиск команды по заданным притериям
                case 3:

                    GuildRoles? role = Converter.FromStrToGuildRole(ans.InlineData);
                    if (role == null)
                    {
                        Logger.Debug($"Converter couldn't convert role. input: {ans.InlineData}");
                        return Respond("Ошибка при распознавании роли. Повторите, пожалуйста, выбор роли.",
                                       GetRolesKeyboard());
                    }

                    memberRole = (GuildRoles)role;

                    avaliableGuilds = await guilds.GetAvaliableGuildsAsync(ans.From.Id);
                    if (avaliableGuilds == null || avaliableGuilds.Count == 0)
                        return MainMenu("К сожалению на данный момент доступных команд для Вас нет.");

                    NextStage();
                    return Respond($"Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                   GenerateKeyboard(guildNavKeyboardMarkup));

                case 4:

                    switch(ans.InlineData)
                    {
                        case "prev_guild":
                            if (guildIter == 0)
                                return Respond($"Выход за пределы. Всего команд: {avaliableGuilds.Count}. Вы просматриваете команду №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                               GenerateKeyboard(guildNavKeyboardMarkup));

                            guildIter--;
                            return Respond($"Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));

                        case "next_guild":
                            if (guildIter == avaliableGuilds.Count - 1)
                                return Respond($"Выход за пределы. Всего команд: {avaliableGuilds.Count}. Вы просматриваете команду №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                               GenerateKeyboard(guildNavKeyboardMarkup));

                            guildIter++;
                            return Respond($"Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));

                        case "request":
                            Request req = new Request()
                            {
                                Name = memberName,
                                Description = memberDescription,
                                From = ans.Chat.Id,
                                To = avaliableGuilds[guildIter].CaptainId,
                                RequestingRole = memberRole
                            };

                            // Проверим, не успел ли подать участник заявку раннее... мало ли...
                            if (!(await reqs.ValidateRequestAsync(req.From, req.To)))
                            {
                                await reqs.SubmitRequestAsync(req);
                                await notify.NotifyAsync(req.To, "В Вашу команду поступила заявка на вступление. Просмотрите активные заявки в меню управления командой.");

                                return MainMenu("Заявка успешно отправлена. Командир команды скоро свяжется с Вами.");
                            }

                            return MainMenu("Вы уже подали заявку на вступление в эту команду");

                        default:
                            return Respond($"Ответ не распознан. Команда №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));
                    }
                    break;

            }

            return MainMenu("Команда не распознана. Возврат в главное меню.");
        }
    }
}
