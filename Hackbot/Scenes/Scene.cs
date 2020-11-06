using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NLog;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using Hackbot.Structures;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Шаблон сцены
    /// </summary>
    public abstract class Scene
    {
        /// <summary>
        /// Обозначает, на каком месте в последний раз диалог был остановлен
        /// </summary>
        protected int Stage
        {
            get
            {
                return stage;
            }
            set
            {
                Logger?.Debug($"Reached stage {stage}");
                stage = value;
            }
        }
        private int stage = 0;

        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        protected Logger Logger { get; set; }

        #region Scene operations

        /// <summary>
        /// Запросить переход к следующей сцене
        /// </summary>
        /// <param name="nextScene">Следующая сцена</param>
        /// <returns></returns>
        protected SceneResult NextScene(SceneTable nextScene) => new SceneResult()
        {
            NextScene = nextScene,
            SceneNextAction = SceneResult.SceneAction.Next
        };

        /// <summary>
        /// Запросить переход к следующей сцене
        /// </summary>
        /// <param name="nextScene">Следующая сцена</param>
        /// <param name="args">Параметры для следующей сцены</param>
        /// <returns></returns>
        protected SceneResult NextScene(SceneTable nextScene, params object[] args) => new SceneResult()
        {
            NextScene = nextScene,
            SceneNextAction = SceneResult.SceneAction.Next,
            NextSceneParams = args
        };

        /// <summary>
        /// Ответить на сообщение пользователя, при этом оставаясь в данной сцене
        /// </summary>
        /// <param name="text">Тест с сообщением</param>
        /// <returns></returns>
        protected SceneResult Respond(string text, IReplyMarkup keyboard) => new SceneResult()
        {
            Answer = text,
            SceneNextAction = SceneResult.SceneAction.Continue,
            KeyboardMarkup = keyboard
        };

        /// <summary>
        /// Сгенерировать ответ, который позволит перейти к главному меню
        /// </summary>
        /// <returns></returns>
        protected SceneResult MainMenu() => new SceneResult()
        {
            Answer = "Возвращение в главное меню",
            SceneNextAction = SceneResult.SceneAction.Zeroize
        };

        /// <summary>
        /// Сгенерировать ответ, который позволит перейти к главному меню
        /// </summary>
        /// <param name="text">Ответ к сообщению</param>
        protected SceneResult MainMenu(string text) => new SceneResult()
        {
            Answer = text,
            SceneNextAction = SceneResult.SceneAction.Zeroize
        };

        #endregion

        #region Stage operations

        /// <summary>
        /// Переход к следующей стадии
        /// </summary>
        protected void NextStage() => Stage++;

        /// <summary>
        /// Переход к заданной стадии
        /// </summary>
        /// <param name="stage"></param>
        protected void ToStage(int stage) => Stage = stage;

        #endregion

        #region Keyboards

        /// <summary>
        /// Вернуть стандартную разметку клавиатуры, которая включает в себя только кнопку для перехода в главное меню
        /// </summary>
        /// <returns></returns>
        protected InlineKeyboardMarkup GetStandardKeyboard() => new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню", "mainmenu")
                }
            });

        /// <summary>
        /// Вернуть клавиатуру с кнопками "да/нет".
        /// </summary>
        /// <returns></returns>
        protected InlineKeyboardMarkup GetYesNoKeyboard() => new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Да", "yes")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Нет", "no")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню", "mainmenu")
                }
            });

        /// <summary>
        /// Вернуть клавиатуру с выбором ролей
        /// </summary>
        /// <returns></returns>
        protected InlineKeyboardMarkup GetRolesKeyboard() => new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Backend разработчик", "role1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Frontend разработчик", "role2"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ГИС специалист", "role3")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Дизайнер", "role4")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Менеджер", "role5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Не важно (или другое)", "OtherRole")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню", "mainmenu")
                }
            });

        /// <summary>
        /// Возвращает клавиатуру с заданными параметрами
        /// </summary>
        /// <param name="args">Словарь с данными о клавиатуре в формате: название_кнопки1, inline_data1, название_кнопки2, inline_data2, название_кнопки3, inline_data3...</param>
        /// <returns>Сгенерированную inline клавиатуру</returns>
        protected InlineKeyboardMarkup GenerateKeyboard(params string[] args)
        {
            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

            for (int i = 0; i < args.Length; i += 2)
                buttons.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData(args[i], args[i + 1]) });

            return new InlineKeyboardMarkup(buttons);
        }

        #endregion

        #region Input recognition and validation

        /// <summary>
        /// Проверить наличие запроса на выход в главное меню
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected bool CheckMenuEscape(RecievedMessage msg) => msg.InlineData == "mainmenu";

        /// <summary>
        /// Проверить сообщение на пустоту
        /// </summary>
        /// <param name="text">Объект сообщения</param>
        /// <returns>true, если сообщение пустое. Иначе false</returns>
        protected bool CheckEmptyMsgText(RecievedMessage text)
        {
            if (string.IsNullOrWhiteSpace(text.Text))
            {
                Logger.Debug($"Message is null or whitespace. chatid: {text.Chat.Id}.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверить результат inline сообщения
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true, если inline равен "no". Иначе false</returns>
        protected bool CheckNegativeInline(RecievedMessage text)
        {
            if (text.InlineData == "no")
            {
                Logger.Debug($"Detected \"no\" inline. chatid: {text.Chat.Id}.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверить результат inline сообщения на нераспознаваемые ответы в случае вопроса "да/нет"
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true, если inline нераспознаваем. Иначе false</returns>
        protected bool DetectYesNoInvalidInline(RecievedMessage text)
        {
            if (text.InlineData != "yes" && text.InlineData != "no")
            {
                Logger.Debug($"Detected invalid inline. chatid: {text.Chat.Id}.");
                return true;
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Генирирует ответ на сообщение пользователя
        /// </summary>
        /// <param name="ans">Ответ пользователя</param>
        /// <returns></returns>
        public abstract Task<SceneResult> GetResult(RecievedMessage ans);
    }
}
