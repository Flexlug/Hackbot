using Hackbot.Services;
using Hackbot.Services.Implementations;
using Hackbot.Structures;
using Hackbot.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Scenes
{
    // TODO Реализовать главное меню участника
    /// <summary>
    /// Главное меню участника какой-либо команды
    /// </summary>
    public class MainMenuMemberScene : Scene
    {
        /// <summary>
        /// Гильдия, капитаном которой является пользователь
        /// </summary>
        private Guild CurrentGuild { get; set; }

        private IGuildsService guilds { get; set; }
        private INotifyService notify { get; set; }

        private string[] keyboardMarkup = new string[] { "Выйти из команды", "leave_guild" };

        private Member member { get; set; }

        public MainMenuMemberScene(long memberId)
        {
            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();

            CurrentGuild = guilds.GetGuildByMemberAsync(memberId).Result;
            Logger = LogManager.GetCurrentClassLogger();
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

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
            {
                ToStage(1);
                return Respond($"Главное меню участника команды: {CurrentGuild.Name}.\n\n{ReturnGuildDescr(CurrentGuild)}",
                               GenerateKeyboard(keyboardMarkup));
            }

            switch (Stage)
            {   
                case 0:
                    member = CurrentGuild.Members.First(x => x.Id == ans.Chat.Id);

                    NextStage();
                    return Respond($"Главное меню участника команды: {CurrentGuild.Name}.\n\n{ReturnGuildDescr(CurrentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                case 1:
                    switch (ans.InlineData)
                    {
                        case "leave_guild":
                            ToStage(2);
                            return Respond($"Вы уверены, что хотите покинуть команду {CurrentGuild.Name}?",
                                           GetYesNoKeyboard());

                        default:
                            return Respond("Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // Выход из команды
                case 2:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"Отменено.\n\n{ReturnGuildDescr(CurrentGuild)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"Ответ не распознан.\n\n{ReturnGuildDescr(CurrentGuild)}",
                                       GetYesNoKeyboard());

                    NextStage();

                    await guilds.RemoveMemberFromGuildAsync(CurrentGuild, member);
                    await notify.NotifyAsync(CurrentGuild.CaptainId, $"{member.Name} покинул Вашу команду.");

                    return MainMenu($"Вы покинули команду {CurrentGuild.Name}");


                default:
                    Logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return Respond($"Ответ не распознан. Возврат к главному меню.\n\n{ReturnGuildDescr(CurrentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }

        // TODO Просмотр сведений о команде
        // TODO Выход из команды
    }
}
