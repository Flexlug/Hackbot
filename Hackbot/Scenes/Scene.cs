using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Hackbot.Structures;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        protected int Stage { get; set; } = 0;

        /// <summary>
        /// Разметка клавиатуры
        /// </summary>
        protected InlineKeyboardMarkup ReplyKeyboard { get; set; }

        /// <summary>
        /// Проверить наличие запроса на выход в главное меню
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected bool CheckMenuEscape(RecievedMessage msg) => msg.InlineData == "mainmenu";

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
        /// Ответить на сообщение пользователя, при этом оставаясь в данной сцене
        /// </summary>
        /// <param name="text">Тест с сообщением</param>
        /// <returns></returns>
        protected SceneResult Respond(string text) => new SceneResult()
        {
            Answer = text,
            SceneNextAction = SceneResult.SceneAction.Continue,
            KeyboardMarkup = ReplyKeyboard
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

        /// <summary>
        /// Генирирует ответ на сообщение пользователя
        /// </summary>
        /// <param name="ans">Ответ пользователя</param>
        /// <returns></returns>
        public abstract Task<SceneResult> GetResult(RecievedMessage ans);
    }
}
