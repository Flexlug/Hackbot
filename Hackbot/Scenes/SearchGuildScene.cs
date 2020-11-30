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
using Centvrio.Emoji;

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

        private IUserGetterService user;

        /// <summary>
        /// Возвращает текстовую информацию о команде
        /// </summary>
        /// <param name="guild">Ссылка на команду</param>
        /// <returns></returns>ы
        private async Task<string> PrintGuildInfo(Guild guild)
        {
            StringBuilder sb = new StringBuilder();

            string userName = (await user.GetUserAsync(guild.CaptainId)).Username;
            if (string.IsNullOrEmpty(userName))
                userName = $"<a href=\"tg://user?id={guild.CaptainId}\">ЛС</a>";
            else
                userName = "@" + userName;

            sb.Append($"{AudioVideo.Play} Название команды: {guild.Name}\n");
            sb.Append($"{AudioVideo.Play} Описание: {guild.Description}\n");
            sb.Append($"{AudioVideo.Play} Капитан: {userName}\n");
            sb.Append($"{AudioVideo.Play} Количество участников: {guild.Members.Count}.\n");

            return sb.ToString();
        }

        private string[] guildNavKeyboardMarkup = new string[] { $"{OtherSymbols.HeavyCheckMark}", "request",
                                                                 $"{AudioVideo.FastReverse}", "prev_guild",
                                                                 $"{AudioVideo.FastForward}", "next_guild",
                                                                 "Меню", "mainmenu" };

        private string memberName = string.Empty;
        private string memberDescription = string.Empty;
        private string memberRole = string.Empty;

        private List<Guild> avaliableGuilds = null;
        private int guildIter = 0;

        public SearchGuildScene()
        {
            Logger = LogManager.GetCurrentClassLogger();

            guilds = GuildsService.GetInstance();
            users = UserGetterService.GetInstance();
            reqs = RequestsService.GetInstance();
            notify = NotifyService.GetInstance();
            user = UserGetterService.GetInstance();
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
                    return Respond($"{Alphanum.Information} Запущен процесс поиска команды. Вам нужно будет заполнить информацию о себе.\n\n{Alphanum.Information} Этап 1 из 3\n{AudioVideo.Play} Введите пожалуйста своё имя.",
                                   GetStandardKeyboard());

                // Проверка имени на валидность
                // Ввод резюме
                case 1:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое имя. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    memberName = ans.Text;

                    NextStage();
                    return Respond($"{Alphanum.Information} Этап 2 из 3\n{AudioVideo.Play} Опишите в 2-3 предложениях свои основные навыки и чем вы можете быть полезны команде.",
                                   GetStandardKeyboard());

                // Проверка резюме на валидность
                // Ввод роли
                case 2:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое описание. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    memberDescription = ans.Text;

                    NextStage();
                    return Respond($"{Alphanum.Information} Этап 3 из 3\n{AudioVideo.Play} Введите роль, которую будете исполнять.",
                                   GetStandardKeyboard());

                // Проверка ввода роли
                // Поиск команды по заданным притериям
                case 3:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое сообщение. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());
                    memberRole = ans.Text;

                    avaliableGuilds = await guilds.GetAvaliableGuildsAsync(ans.From.Id);
                    if (avaliableGuilds == null || avaliableGuilds.Count == 0)
                        return MainMenu($"{Geometric.RedCircle} К сожалению на данный момент доступных команд для Вас нет.");

                    NextStage();
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                   GenerateKeyboard(guildNavKeyboardMarkup));

                case 4:

                    switch(ans.InlineData)
                    {
                        case "prev_guild":
                            if (guildIter == 0)
                                return Respond($"{OtherSymbols.Exclamation} Выход за пределы. Всего команд: {avaliableGuilds.Count}. Вы просматриваете команду №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                               GenerateKeyboard(guildNavKeyboardMarkup));

                            guildIter--;
                            return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));

                        case "next_guild":
                            if (guildIter == avaliableGuilds.Count - 1)
                                return Respond($"{OtherSymbols.Exclamation} Выход за пределы. Всего команд: {avaliableGuilds.Count}. Вы просматриваете команду №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                               GenerateKeyboard(guildNavKeyboardMarkup));

                            guildIter++;
                            return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Надено подходящих команд: {avaliableGuilds.Count}.\nКоманда №{guildIter + 1}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));

                        case "request":
                            Request req = new Request()
                            {
                                Name = memberName,
                                Description = memberDescription,
                                From = ans.Chat.Id,
                                To = avaliableGuilds[guildIter].CaptainId,
                                Role = memberRole
                            };

                            // Проверим, не успел ли подать участник заявку раннее... мало ли...
                            if (!(await reqs.ValidateRequestAsync(req.From, req.To)))
                            {
                                await reqs.SubmitRequestAsync(req);
                                await notify.NotifyAsync(req.To, $"{OtherSymbols.WhiteHeavyCheckMark} В Вашу команду поступила заявка на вступление. Просмотрите активные заявки в меню управления командой.");

                                return MainMenu($"{OtherSymbols.WhiteHeavyCheckMark} Заявка успешно отправлена. Командир команды скоро свяжется с Вами.");
                            }

                            return MainMenu($"{OtherSymbols.CrossMark} Вы уже подали заявку на вступление в эту команду");

                        default:
                            return Respond($"{OtherSymbols.Question} Ответ не распознан. Команда №{guildIter + 1} из {avaliableGuilds.Count}\n\n{await PrintGuildInfo(avaliableGuilds[guildIter])}",
                                           GenerateKeyboard(guildNavKeyboardMarkup));
                    }
            }

            return MainMenu($"{OtherSymbols.Question} Команда не распознана. Возврат в главное меню.");
        }
    }
}
