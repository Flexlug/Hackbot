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
        private ISceneControllerNotifyer controllerNotifyer { get; set; }

        private string[] keyboardMarkup = new string[] { "Изменить описание", "change_descr",
                                                         "Просмотреть заявки", "watch_reqs",
                                                         "Удалить участника", "remove_member",
                                                         "Удалить команду", "remove_guild",
                                                         "Главное меню", "mainmenu" };

        public CaptainGuildEditScene(Guild currentGuild)
        {
            Logger = LogManager.GetCurrentClassLogger();

            this.currentGuild = currentGuild;

            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();
            reqs = RequestsService.GetInstance();
            controllerNotifyer = SceneControllerNotifyer.GetInstance();
        }

        /// <summary>
        /// Вернуть описание команды
        /// </summary>
        /// <param name="g">Ссылка на команду</param>
        /// <returns></returns>
        private string ReturnGuildDescr(Guild g)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Название команды: {g.Name}");
            sb.AppendLine($"Описание команды: {g.Description}\n");
            sb.AppendLine($"Количество участников: {g.Members.Count}");

            for (int i = 0; i < g.Members.Count; i++)
            {
                sb.AppendLine($"№{i + 1}: {g.Members[i].Name} ({Converter.GuildRoleToStr(g.Members[i].Role)})"); 
                sb.AppendLine($" - {g.Members[i].Description}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Вернуть всех участников команды
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private string ReturnGuildTeam(Guild g)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < currentGuild.Members.Count; i++)
                sb.AppendLine($"№{i + 1}: {g.Members[i].Name} ({Converter.GuildRoleToStr(g.Members[i].Role)}) {(g.Members[i].Id == g.CaptainId ? "CAP" : "")}");

            return sb.ToString();
        }


        private string newDescription;
        private Member deletingMem;

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
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
                    return Respond($"Панель управления командой: \n{ReturnGuildDescr(currentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                // ГЛАВНОЕ МЕНЮ
                case 1:
                    switch (ans.InlineData)
                    {
                        case "change_descr":
                            ToStage(4);
                            return Respond("Введите пожалуйста новое описание команды.",
                                           GetStandardKeyboard("guildmenu"));

                        case "watch_reqs":
                            return NextScene(SceneTable.CaptainRequestsView, currentGuild);

                        case "remove_member":
                            ToStage(2);
                            return Respond($"Введите пожалуйста порядковый номер исключаемого участника.\n{ReturnGuildTeam(currentGuild)}",
                                           GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                        case "remove_guild":
                            ToStage(6);
                            return Respond($"Вы уверены, что хотите удалить команду {currentGuild.Name}? Действие будет невозможно отменить!\nДля удаления введите название команды.",
                                           GetStandardKeyboard("guildmenu"));

                        default:
                            return Respond($"Ответ не распознан.\nПанель управления командой: \n{ReturnGuildDescr(currentGuild)}",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // Проверка порядкового номера
                // Подтверждение удаления
                case 2:
                    int outp = -1;
                    if (!(int.TryParse(ans.InlineData, out outp) && outp <= currentGuild.Members.Count && outp > 0))
                        return Respond("Ответ не распознан. Повторите выбор участника.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                    NextStage();

                    deletingMem = currentGuild.Members[outp - 1];

                    if (deletingMem.Id == currentGuild.CaptainId)
                    {
                        ToStage(1);
                        return Respond($"Вы не можете исключить самого себя\n\n{ReturnGuildDescr(currentGuild)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    return Respond($"Вы собираетесь удалить из команды следующего участника:\nИмя: {deletingMem.Name}\nРоль: {Converter.GuildRoleToStr(deletingMem.Role)}\nОписание: {deletingMem.Description}\n\nВы уверены?",
                                   GetYesNoKeyboard("guildmenu"));

                // Удаление участника или отмена этого действия
                case 3:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"Отмена\n\n{ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond("Ответ не распознан. Повторите ещё раз.",
                                       GetYesNoKeyboard("guildmenu"));

                    await guilds.RemoveMemberFromGuildAsync(currentGuild, deletingMem);
                    await notify.NotifyAsync(deletingMem.Id, $"Вы были исключены из команды: {currentGuild.Name}");
                    await controllerNotifyer.RemoveUserDialogAsync(deletingMem.Id);

                    return Respond($"Участник удалён из команды.\n\n{ReturnGuildDescr(currentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                // ВВОД НОВОГО ОПИСАНИЯ
                // Подтверждение изменения
                case 4:
                    if (CheckEmptyMsgText(ans))
                        return Respond("Введён пустой текст.",
                                       GetStandardKeyboard("guildmenu"));

                    newDescription = ans.Text;

                    NextStage();
                    return Respond($"Вы уверены?", GetYesNoKeyboard("guildmenu"));

                // Валидация да/нет
                // Изменение описания
                case 5:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"Отмена\n\n{ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond("Ответ не распознан. Повторите, пожалуйста, ввод.", GetYesNoKeyboard("guildmenu"));

                    await guilds.ChangeGuildDescriptionAsync(currentGuild, newDescription);
                    currentGuild.Description = newDescription;

                    ToStage(1);
                    return Respond($"Описание изменено.\n\n{ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));


                // УДАЛЕНИЕ команды
                // Проверка на валидность и удаление
                case 6:

                    if (CheckEmptyMsgText(ans) || ans.Text != currentGuild.Name)
                    {
                        ToStage(1);
                        return Respond($"Отмена\n\n{ReturnGuildDescr(currentGuild)}", GenerateKeyboard(keyboardMarkup));
                    }

                    await guilds.RemoveGuildAsync(currentGuild);

                    foreach (Request req in await reqs.GetRequestsByCaptainIdAsync(currentGuild.CaptainId))
                        await notify.NotifyAsync(req.From, $"Команда {currentGuild.Name}, в которую Вы подавали заявку на вступление, была удалена.");

                    await reqs.RemoveRequestsByCaptainIdAsync(currentGuild.CaptainId);

                    foreach (Member mem in currentGuild.Members)
                    {
                        await notify.NotifyAsync(mem.Id, $"Команда {currentGuild.Name}, в которой Вы состояли, была удалена.");
                        await controllerNotifyer.RemoveUserDialogAsync(mem.Id);
                    }

                    return MainMenu("Команда удалена");

                default:
                    return Respond($"Ответ не распознан.\nПанель управления командой: \n{ReturnGuildDescr(currentGuild)}",
                       GenerateKeyboard(keyboardMarkup));
            }


        }
    }
}
