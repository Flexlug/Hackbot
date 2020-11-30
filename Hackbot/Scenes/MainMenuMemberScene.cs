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

using Centvrio.Emoji;

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
        private IUserGetterService user { get; set; }

        private string[] keyboardMarkup = new string[] { $"{OtherSymbols.CrossMark} Выйти из команды", "leave_guild" };

        private Member member { get; set; }

        public MainMenuMemberScene(long memberId)
        {
            guilds = GuildsService.GetInstance();
            notify = NotifyService.GetInstance();
            user = UserGetterService.GetInstance();

            CurrentGuild = guilds.GetGuildByMemberAsync(memberId).Result;
            Logger = LogManager.GetCurrentClassLogger();
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
            sb.AppendLine($"{AudioVideo.Play} Количество участников: {g.Members.Count}");

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

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            if (CheckMenuEscape(ans))
            {
                ToStage(1);
                return Respond($"{Alphanum.Information} Главное меню участника команды: {CurrentGuild.Name}.\n\n{await ReturnGuildDescr(CurrentGuild)}",
                               GenerateKeyboard(keyboardMarkup));
            }

            switch (Stage)
            {   
                case 0:
                    member = CurrentGuild.Members.First(x => x.Id == ans.Chat.Id);

                    NextStage();
                    return Respond($"{Alphanum.Information} Главное меню участника команды: {CurrentGuild.Name}.\n\n{await ReturnGuildDescr(CurrentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));

                case 1:
                    switch (ans.InlineData)
                    {
                        case "leave_guild":
                            ToStage(2);
                            return Respond($"{OtherSymbols.Question} Вы уверены, что хотите покинуть команду {CurrentGuild.Name}?",
                                           GetYesNoKeyboard());

                        default:
                            return Respond($"{OtherSymbols.Question} Ответ не распознан.",
                                           GenerateKeyboard(keyboardMarkup));
                    }

                // Выход из команды
                case 2:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.Question} Отменено.\n\n{await ReturnGuildDescr(CurrentGuild)}",
                                       GenerateKeyboard(keyboardMarkup));
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"{OtherSymbols.Question} Ответ не распознан.\n\n{await ReturnGuildDescr(CurrentGuild)}",
                                       GetYesNoKeyboard());

                    NextStage();

                    await guilds.RemoveMemberFromGuildAsync(CurrentGuild, member);
                    await notify.NotifyAsync(CurrentGuild.CaptainId, $"{OtherSymbols.CrossMark} {member.Name} покинул Вашу команду.");

                    return MainMenu($"{OtherSymbols.CrossMark} Вы покинули команду {CurrentGuild.Name}");


                default:
                    Logger.Debug($"Unrecognized stage. chatid: {ans.Chat.Id}");
                    return Respond($"{OtherSymbols.Question} Ответ не распознан. Возврат к главному меню.\n\n{await ReturnGuildDescr(CurrentGuild)}",
                                   GenerateKeyboard(keyboardMarkup));
            }
        }

        // TODO Просмотр сведений о команде
        // TODO Выход из команды
    }
}
