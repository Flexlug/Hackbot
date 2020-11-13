using Hackbot.Services;
using Hackbot.Services.Implementations;
using Hackbot.Structures;
using Hackbot.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        private ISceneControllerNotifyer controllerNotifyer { get; set; }
        private Guild currentGuild { get; set; }
        private List<Request> requests { get; set; }

        private string[] keyboardMarkup = new string[] { "Принять заявку", "accept",
                                                         "Отклонить заявку", "decline",
                                                         "Назад", "toGuildEdit"};

        /// <summary>
        /// Отобразить коллекцию заявок
        /// </summary>
        /// <param name="reqs">Коллекция заявок</param>
        /// <returns></returns>
        private string PrintRequests(List<Request> reqs)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < reqs.Count; i++)
                sb.AppendLine($"№{i + 1}\nОт: {reqs[i].Name} на роль: {Converter.GuildRoleToStr(reqs[i].RequestingRole)}\nСкиллы: {reqs[i].Description}");

            return sb.ToString();
        }

        public CaptainRequestsViewScene(Guild currentGuild)
        {
            Logger = LogManager.GetCurrentClassLogger();

            this.currentGuild = currentGuild;

            reqs = RequestsService.GetInstance();
            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();
            controllerNotifyer = SceneControllerNotifyer.GetInstance();
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
                return MainMenu();

            switch (Stage)
            {
                case 0:
                    requests = await reqs.GetRequestsByCaptainIdAsync(currentGuild.CaptainId);

                    NextStage();
                    if (requests == null || requests.Count == 0)
                        return Respond($"Активных заявок нет.",
                                       GetStandardKeyboard("toGuildEdit"));

                    return Respond($"Активные заявки на вступление: \n{PrintRequests(requests)}",
                                   GenerateKeyboard(keyboardMarkup));

                // МЕНЮ ВЫБОРА КОМАНД
                case 1:

                    switch (ans.InlineData)
                    {
                        case "toGuildEdit":
                            return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                        case "accept":
                            ToStage(2);
                            return Respond("Выберете номер заявки.",
                                           GenerateKeyboard(GetNumericMarkup(requests.Count)));

                        case "decline":
                            ToStage(3);
                            return Respond("Выберете номер заявки.",
                                           GenerateKeyboard(GetNumericMarkup(requests.Count)));

                        default:
                            return Respond("Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // ПРИЁМ ЗАЯВКИ
                // Получение номера заявки и её приём
                case 2:
                    int outp = -1;
                    if (!(int.TryParse(ans.InlineData, out outp) && outp <= currentGuild.Members.Count && outp > 0))
                        return Respond("Ответ не распознан. Повторите выбор заявки.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                    Request req = requests[outp - 1];
                    Member mem = new Member()
                    {
                        Id = req.From,
                        Name = req.Name,
                        Description = req.Description,
                        Role = req.RequestingRole
                    };

                    if (!await reqs.ValidateRequestAsync(req.From, req.To))
                    {
                        ToStage(1);
                        return Respond($"Заявка больше недействительна. Возможно пользователя уже приняли в другую команду.\n{PrintRequests(requests)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    currentGuild.Members.Add(mem);
                    await reqs.RemoveRequestsByMemberIdAsync(req.From);
                    await guilds.AddMemberToGuildAsync(currentGuild, mem);      // Обнулим все остальные заявки от этого участника
                    requests.Remove(req);

                    await notify.NotifyAsync(req.From, $"Вы были приняты в команду {currentGuild.Name}!");
                    await controllerNotifyer.RemoveUserDialogAsync(req.From);

                    if (requests.Count == 0)
                        return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                    ToStage(1);
                    return Respond($"Заявка принята.\n\n{PrintRequests(requests)}", 
                                   GenerateKeyboard(keyboardMarkup));

                // ОТКЛОНЕНИЕ ЗАЯВКИ
                // Получение номера заявки и её приём
                case 3:
                    int outpp = -1;
                    if (!(int.TryParse(ans.InlineData, out outpp) && outpp <= currentGuild.Members.Count && outpp > 0))
                        return Respond("Ответ не распознан. Повторите выбор заявки.",
                                       GenerateKeyboard(GetNumericMarkup(currentGuild.Members.Count)));

                    Request reqq = requests[outpp - 1];

                    if (!await reqs.ValidateRequestAsync(reqq.From, reqq.To))
                    {
                        ToStage(1);
                        return Respond("Заявка больше недействительна. Возможно пользователя уже приняли в другую команду.",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    await reqs.RevokeRequestAsync(reqq.From, reqq.To);
                    requests.Remove(reqq);

                    await notify.NotifyAsync(reqq.From, $"Ваша заявка была отклонена командиром команды {currentGuild.Name}.");

                    if (requests.Count == 0)
                        return NextScene(SceneTable.CaptainGuildEditScene, currentGuild);

                    ToStage(1);
                    return Respond($"Заявка отклонена.\n\n{PrintRequests(requests)}",
                                   GenerateKeyboard(keyboardMarkup));


                default:
                    Logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return MainMenu("Ответ не распознан. Возврат к главному меню.");
            }
        }

        // TODO Принять заявку
        // TODO Редактировать выбранную заявку
    }
}
