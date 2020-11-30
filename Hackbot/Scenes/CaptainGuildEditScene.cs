using Centvrio.Emoji;
using Hackbot.Services;
using Hackbot.Services.Implementations;
using Hackbot.Structures;
using Hackbot.Util;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Scenes
{
    // TODO Реализовать меню управления командой
    /// <summary>
    /// Меню управления командой от лица командира
    /// </summary>
    public class CaptainGuildEditScene : Scene
    {
        /// <summary>
        /// Управляемая гильдия
        /// </summary>
        private Guild currentGuild { get; set; }

        private IGuildsService guilds { get; set; }
        private IRequestsService reqs { get; set; }
        private INotifyService notify { get; set; }
        private IUserGetterService user { get; set; }
        private ISceneControllerNotifyer controllerNotifyer { get; set; }

        private string[] keyboardMarkup = new string[] { $"{BookPaper.Label} Изменить описание", "change_descr",
                                                         $"{BookPaper.PageFacingUp} Просмотреть заявки", "watch_reqs",
                                                         $"{OtherSymbols.CrossMark} Удалить участника", "remove_member",
                                                         $"{OtherSymbols.CrossMark} Удалить команду", "remove_guild",
                                                         "Главное меню", "mainmenu" };

        public CaptainGuildEditScene(Guild currentGuild)
        {
            Logger = LogManager.GetCurrentClassLogger();

            this.currentGuild = currentGuild;

            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();
            reqs = RequestsService.GetInstance();
            user = UserGetterService.GetInstance();
            controllerNotifyer = SceneControllerNotifyer.GetInstance();
        }

        /// <summary>
        /// Вернуть описание команды
        /// </summary>
        /// <param name="g">Ссылка на команду</param>
        /// <returns></returns>
        private async Task<string> ReturnGuildDescr(Guild g)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{AudioVideo.Play} Название команды: {g.Name}");
            sb.AppendLine($"{AudioVideo.Play} Описание команды: {g.Description}\n");
            sb.AppendLine($"Количество участников: {g.Members.Count}");

            for (int i = 0; i < g.Members.Count; i++)
            {
                string userName = (await user.GetUserAsync(g.Members[i].Id)).Username;
                if (string.IsNullOrEmpty(userName))
                    userName = $"<a href=\"tg://user?id={g.Members[i].Id}\">ЛС</a>";
                else
                    userName = "@" + userName;

                sb.AppendLine($"{EmojiHelp.Digit(i + 1)}: {g.Members[i].Name} ({g.Members[i].Role})");
                sb.AppendLine($" - {userName}");
                sb.AppendLine($" - {g.Members[i].Description}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Вернуть всех участников команды
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private async Task<string> ReturnGuildTeam(Guild g)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < g.Members.Count; i++)
            {
                string userName = (await user.GetUserAsync(g.Members[i].Id))?.Username;
                if (string.IsNullOrEmpty(userName))
                    userName = $"<a href=\"tg://user?id={g.Members[i].Id}\">ЛС</a>";
                else
                    userName = "@" + userName;

                sb.AppendLine($"{EmojiHelp.Digit(i + 1)}: {userName} ({g.Members[i].Role}) {(g.Members[i].Id == g.CaptainId ? "CAP" : "")}");
            }

            return sb.ToString();
        }


        private string newDescription;
        private Member deletingMem;

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            // Постоянно обновляем данные о гильдии
            currentGuild = await guilds.GetGuildByCaptianAsync(currentGuild.CaptainId);

            if (CheckMenuEscape(ans))
                return MainMenu();

            if (ans.InlineData == "guildmenu")
                ToStage(0);

            // Обработка ответа
            switch (Stage)
            {
                // Вход в меню
                case 0:
                    NextStage();
                    return Respond($"{Tool.Wrench} Панель управления командой: \n\n{await ReturnGuildDescr(currentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                // ГЛАВНОЕ МЕНЮ
                case 1:
                    switch (ans.InlineData)
                    {
                        case "change_descr":
                            ToStage(4);
                            return Respond($"{AudioVideo.Play} Введите пожалуйста новое описание команды.",
                                           GetStandardKeyboard("guildmenu"));

                        case "watch_reqs":
                            return NextScene(SceneTable.CaptainRequestsView, currentGuild);

                        case "remove_member":
                            ToStage(2);
                            return Respond($"{AudioVideo.Play} Введите пожалуйста порядковый номер исключаемого участника.\n{await ReturnGuildTeam(currentGuild)}",
                                           GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                        case "remove_guild":
                            ToStage(6);
                            return Respond($"{OtherSymbols.ExclamationQuestion} Вы уверены, что хотите удалить команду {currentGuild.Name}? Действие будет невозможно отменить!\nДля удаления введите название команды.",
                                           GetStandardKeyboard("guildmenu"));

                        default:
                            return Respond($"{OtherSymbols.Question} Ответ не распознан.\n{Tool.Wrench} Панель управления командой: \n\n{await ReturnGuildDescr(currentGuild)}",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // Проверка порядкового номера
                // Подтверждение удаления
                case 2:
                    int outp = -1;
                    if (!(int.TryParse(ans.InlineData, out outp) && outp <= currentGuild.Members.Count && outp > 0))
                        return Respond($"{OtherSymbols.Question} Ответ не распознан. Повторите выбор участника.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                    NextStage();

                    deletingMem = currentGuild.Members[outp - 1];

                    if (deletingMem.Id == currentGuild.CaptainId)
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Exclamation} Вы не можете исключить самого себя\n\n{await ReturnGuildDescr(currentGuild)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }


                    string userName = (await user.GetUserAsync(deletingMem.Id))?.Username;
                    if (string.IsNullOrEmpty(userName))
                        userName = $"<a href=\"tg://user?id={deletingMem?.Id}\">ЛС</a>";
                    else
                        userName = "@" + userName;

                    return Respond($"{OtherSymbols.Question} Вы собираетесь удалить из команды следующего участника:\n\n{AudioVideo.Play} Имя: {deletingMem.Name}\n{AudioVideo.Play} {userName}\n{AudioVideo.Play} Роль: {deletingMem.Role}\n{AudioVideo.Play} Описание: {deletingMem.Description}\n\nВы уверены?",
                                   GetYesNoKeyboard("guildmenu"));

                // Удаление участника или отмена этого действия
                case 3:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.CrossMark} Отмена\n\n{await ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"{OtherSymbols.Question} Ответ не распознан. Повторите ещё раз.",
                                       GetYesNoKeyboard("guildmenu"));

                    currentGuild.Members.Remove(
                            currentGuild.Members.FirstOrDefault(x => x.Id == deletingMem.Id)
                        );

                    await guilds.RemoveMemberFromGuildAsync(currentGuild, deletingMem);
                    await notify.NotifyAsync(deletingMem.Id, $"{OtherSymbols.CrossMark} Вы были исключены из команды: {currentGuild.Name}");
                    await controllerNotifyer.RemoveUserDialogAsync(deletingMem.Id);

                    return Respond($"{OtherSymbols.CrossMark} Участник удалён из команды.\n\n{await ReturnGuildDescr(currentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                // ВВОД НОВОГО ОПИСАНИЯ
                // Подтверждение изменения
                case 4:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.Question} Введён пустой текст.",
                                       GetStandardKeyboard("guildmenu"));

                    newDescription = ans.Text;

                    NextStage();
                    return Respond($"{AudioVideo.Play} Вы уверены?", GetYesNoKeyboard("guildmenu"));

                // Валидация да/нет
                // Изменение описания
                case 5:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Question} Отмена\n\n{await ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"{OtherSymbols.Question} Ответ не распознан. Повторите, пожалуйста, ввод.", GetYesNoKeyboard("guildmenu"));

                    await guilds.ChangeGuildDescriptionAsync(currentGuild, newDescription);
                    currentGuild.Description = newDescription;

                    ToStage(1);
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Описание изменено.\n\n{await ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));


                // УДАЛЕНИЕ команды
                // Проверка на валидность и удаление
                case 6:

                    if (CheckEmptyMsgText(ans) || ans.Text != currentGuild.Name)
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Question} Отмена\n\n{await ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    await guilds.RemoveGuildAsync(currentGuild);

                    foreach (Request req in await reqs.GetRequestsByCaptainIdAsync(currentGuild.CaptainId))
                        await notify.NotifyAsync(req.From, $"{OtherSymbols.CrossMark} Команда {currentGuild.Name}, в которую Вы подавали заявку на вступление, была удалена.");

                    await reqs.RemoveRequestsByCaptainIdAsync(currentGuild.CaptainId);

                    foreach (Member mem in currentGuild.Members)
                    {
                        await notify.NotifyAsync(mem.Id, $"{OtherSymbols.CrossMark} Команда {currentGuild.Name}, в которой Вы состояли, была удалена.");
                        await controllerNotifyer.RemoveUserDialogAsync(mem.Id);
                    }

                    return MainMenu($"{OtherSymbols.CrossMark} Команда удалена");

                default:
                    return Respond($"{OtherSymbols.Question} Ответ не распознан.\n{Tool.Wrench} Панель управления командой: \n\n{await ReturnGuildDescr(currentGuild)}",
                       GenerateKeyboard(keyboardMarkup));
            }


        }
    }
}
