using Hackbot.Services;
using Hackbot.Services.Implementations;
using Hackbot.Structures;
using Hackbot.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Centvrio.Emoji;

namespace Hackbot.Scenes
{
    // TODO Реализовать меню просмотра поданых заявок на вступление в команды
    /// <summary>
    /// Меню просмотра поданых заявок на вступление в команды
    /// </summary>
    public class CaptainRequestsViewScene : Scene
    {
        private IRequestsService reqs { get; set; }
        private IGuildsService guilds { get; set; }
        private INotifyService notify { get; set; }
        private IUserGetterService user { get; set; }
        private ISceneControllerNotifyer controllerNotifyer { get; set; }
        private Guild currentGuild { get; set; }
        private List<Request> requests { get; set; }

        private string[] keyboardMarkup = new string[] { $"{OtherSymbols.WhiteHeavyCheckMark}", "accept",
                                                         $"{OtherSymbols.CrossMark}", "decline",
                                                         "Назад", "toGuildEdit"};

        /// <summary>
        /// Отобразить коллекцию заявок
        /// </summary>
        /// <param name="reqs">Коллекция заявок</param>
        /// <returns></returns>
        private async Task<string> PrintRequests(List<Request> reqs)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < reqs.Count; i++) {
                string userName = (await user.GetUserAsync(reqs[i].From))?.Username;
                if (string.IsNullOrEmpty(userName))
                    userName = $"<a href=\"tg://user?id={reqs[i].From}\">ЛС</a>";
                else
                    userName = "@" + userName;

                sb.AppendLine($"{EmojiHelp.Digit(i + 1)}\nОт: {reqs[i].Name} на роль: {reqs[i].Role}\nСкиллы: {reqs[i].Description}\n{userName}");
            }

            return sb.ToString();
        }

        public CaptainRequestsViewScene(Guild currentGuild)
        {
            Logger = LogManager.GetCurrentClassLogger();

            this.currentGuild = currentGuild;

            reqs = RequestsService.GetInstance();
            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();
            user = UserGetterService.GetInstance();
            controllerNotifyer = SceneControllerNotifyer.GetInstance();
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
                return MainMenu();

            if (ans.InlineData == "toGuildEdit")
                return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);


            switch (Stage)
            {
                case 0:
                    requests = await reqs.GetRequestsByCaptainIdAsync(currentGuild.CaptainId);

                    NextStage();
                    if (requests == null || requests.Count == 0)
                        return Respond($"{Geometric.RedCircle} Активных заявок нет.",
                                       GetStandardKeyboard("toGuildEdit"));

                    return Respond($"{Geometric.GreenCircle} Активные заявки на вступление: \n{await PrintRequests(requests)}",
                                   GenerateKeyboard(keyboardMarkup));

                // МЕНЮ ВЫБОРА КОМАНД
                case 1:

                    switch (ans.InlineData)
                    {
                        case "toGuildEdit":
                            return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                        case "accept":
                            ToStage(2);
                            return Respond($"{AudioVideo.Play} Выберете номер заявки.",
                                           GenerateKeyboard(GetNumericMarkup(requests.Count, "toGuildEdit")));

                        case "decline":
                            ToStage(3);
                            return Respond($"{AudioVideo.Play} Выберете номер заявки.",
                                           GenerateKeyboard(GetNumericMarkup(requests.Count, "toGuildEdit")));

                        default:
                            return Respond($"{OtherSymbols.Question} Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // ПРИЁМ ЗАЯВКИ
                // Получение номера заявки и её приём
                case 2:
                    int outp = -1;
                    if (!(int.TryParse(ans.InlineData, out outp) && outp <= currentGuild.Members.Count && outp > 0))
                        return Respond($"{OtherSymbols.Question} Ответ не распознан. Повторите выбор заявки.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count, "toGuildEdit")));

                    Request req = requests[outp - 1];
                    Member mem = new Member()
                    {
                        Id = req.From,
                        Name = req.Name,
                        Description = req.Description,
                        Role = req.Role
                    };

                    if (!await reqs.ValidateRequestAsync(req.From, req.To))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Exclamation} Заявка больше недействительна. Возможно пользователя уже приняли в другую команду.\n{await PrintRequests(requests)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    currentGuild.Members.Add(mem);
                    await reqs.RemoveRequestsByMemberIdAsync(req.From);
                    await guilds.AddMemberToGuildAsync(currentGuild, mem);      // Обнулим все остальные заявки от этого участника
                    requests.Remove(req);

                    await notify.NotifyAsync(req.From, $"{OtherSymbols.WhiteHeavyCheckMark} Вы были приняты в команду {currentGuild.Name}!");
                    await controllerNotifyer.RemoveUserDialogAsync(req.From);

                    if (requests.Count == 0)
                        return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                    ToStage(1);
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Заявка принята.\n\n{await PrintRequests(requests)}", 
                                   GenerateKeyboard(keyboardMarkup));

                // ОТКЛОНЕНИЕ ЗАЯВКИ
                // Получение номера заявки и её приём
                case 3:
                    int outpp = -1;
                    if (!(int.TryParse(ans.InlineData, out outpp) && outpp <= currentGuild.Members.Count && outpp > 0))
                        return Respond($"{OtherSymbols.Question}  Ответ не распознан. Повторите выбор заявки.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count, "toGuildEdit")));

                    Request reqq = requests[outpp - 1];

                    if (!await reqs.ValidateRequestAsync(reqq.From, reqq.To))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Exclamation} Заявка больше недействительна. Возможно пользователя уже приняли в другую команду.",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    await reqs.RevokeRequestAsync(reqq.From, reqq.To);
                    requests.Remove(reqq);

                    await notify.NotifyAsync(reqq.From, $"{OtherSymbols.CrossMark} Ваша заявка была отклонена командиром команды {currentGuild.Name}.");

                    if (requests.Count == 0)
                        return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                    ToStage(1);
                    return Respond($"{OtherSymbols.CrossMark} Заявка отклонена.\n\n{await PrintRequests(requests)}",
                                   GenerateKeyboard(keyboardMarkup));


                default:
                    Logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return MainMenu($"{OtherSymbols.Question} Ответ не распознан. Возврат к главному меню.");
            }
        }

        // TODO Принять заявку
        // TODO Редактировать выбранную заявку
    }
}
