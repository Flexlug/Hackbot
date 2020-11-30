using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


using Hackbot.Services;
using Hackbot.Structures;
using Hackbot.Services.Implementations;

using Centvrio.Emoji;
using Hackbot.Util;

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
        private string captainRole = string.Empty;
        private string guildName = string.Empty;
        private string guildDescription = string.Empty;

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
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Запущен процесс регистрации команды.\nВам нужно будет заполнить информацию о себе и новой команде.\n\n{AudioVideo.Play} Начнём с Вас.\n\n{Alphanum.Information} Этап 1 из 2\n{AudioVideo.Play} Введите пожалуйста своё имя.",
                                   GetStandardKeyboard());

                // Проверка имени на валидность
                // Ввод Member.Description
                case 1:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое имя. Повторите, пожалуйста, ещё раз.",
                                       GetStandardKeyboard());

                    captainName = ans.Text;

                    NextStage();
                    return Respond($"{Alphanum.Information} Этап 2 из 2\n{AudioVideo.Play} Опишите в 2-3 предложениях свои основные навыки и чем вы можете быть полезны команде.",
                                   GetStandardKeyboard());

                // Проверка Member.Description на валидность
                // Вопрос о правильности введённых данных
                case 2:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое сообщение. Не стесняйтесь написать хоть что-нибудь о себе. В противном же случае просто напишите \"капитан\" =).",
                                       GetStandardKeyboard());

                    captainDescrption = ans.Text;

                    NextStage();
                    return Respond($"{OtherSymbols.Question} Подтвердите правильность введённых данных\n\n{AudioVideo.Play} Ваше имя: {captainName}\n{AudioVideo.Play} Ваши навыки: {captainDescrption}",
                                   GetYesNoKeyboard());

                // Yes/No на вопрос о правильности составленного "резюме"
                // Выбор названия команды
                case 3:
                    if (CheckNegativeInline(ans))
                    {
                        ToStage(1);
                        return Respond($"{OtherSymbols.CrossMark} Процесс ввода ваших данных запущен заново. Введите пожалуйста своё имя.",
                                       GetStandardKeyboard());
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"{OtherSymbols.CrossMark} Ответ не распознан.\n\n{AudioVideo.Play} Ваше имя: {captainName}\n{AudioVideo.Play} Ваши навыки: {captainDescrption}\n\n\nПодтвердите правильность введённых данных.",
                                       GetYesNoKeyboard());

                    NextStage();
                    return Respond($"{OtherSymbols.WhiteHeavyCheckMark} Теперь введем данные о новой команде.\n\n{Alphanum.Information} Этап 1 из 3\n{AudioVideo.Play} Введите название команды.",
                                   GetStandardKeyboard());

                // Проверка названия команды на валидность
                // Выбор описания команды
                case 4:

                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое название команды. Повторите ещё раз.",
                                       GetStandardKeyboard());

                    guildName = ans.Text;

                    NextStage();
                    return Respond($"{Alphanum.Information} Этап 2 из 3\n{AudioVideo.Play} Введите краткое описание команды. Укажите, кто нужен команде. В каком направлении скорее всего команда будет работать.",
                                   GetStandardKeyboard());

                // Проверка описания команды на валидность
                // Выбор роли
                case 5:

                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое описание команды. Повторите ещё раз.",
                                       GetStandardKeyboard());

                    guildDescription = ans.Text;

                    NextStage();
                    return Respond($"{Alphanum.Information} Этап 3 из 3\n{AudioVideo.Play} Какую роль вы будете исполнять в команде (помимо капитанской)?.",
                                   GetStandardKeyboard());

                // Проверка введенной роли на валидность
                // Подтверждение введённых данных о команде
                case 6:
                    if (CheckEmptyMsgText(ans))
                        return Respond($"{OtherSymbols.CrossMark} Вы ввели пустое сообщение. Повторите ещё раз.",
                                       GetStandardKeyboard());

                    captainRole = ans.Text;

                    NextStage();
                    return Respond($"{OtherSymbols.Question}Подтвердите правильность введённых данных\n\n{AudioVideo.Play} Название команды: {guildName}\n{AudioVideo.Play} Описание команды: {guildDescription}\n{AudioVideo.Play} Ваша роль: {captainRole}\n\n",
                                   GetYesNoKeyboard());

                // Подтверждение введённых данных о команде. Создание команды
                case 7:
                    Logger.Debug($"Reached stage {Stage}. chatid: {ans.Chat.Id}");

                    if (CheckNegativeInline(ans))
                    {
                        ToStage(3);
                        return Respond($"{Alphanum.Information} Процесс регистрации команды запущен заново.\n\n{Alphanum.Information} Этап 1 из 2\n{AudioVideo.Play} Введите пожалуйста название команды.",
                                       GetStandardKeyboard());
                    }

                    if (DetectYesNoInvalidInline(ans))
                        return Respond($"{OtherSymbols.CrossMark} Ответ не распознан.\n\nВаше имя: {captainName}\nНазвание команды: {guildName}\nВаша роль: {captainRole}\n\nПодтвердите правильность введённых данных.",
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

                    return MainMenu($"Команда \"{guild.Name}\" успешно создана. {FaceRole.Partying}");

                default:
                    return Respond($"{OtherSymbols.CrossMark} Ответ не распознан.",
                                   GetStandardKeyboard());
            }
        }
    }
}
