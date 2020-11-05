using System;
using System.Collections.Generic;
using System.Text;

using Telegram.Bot.Types.ReplyMarkups;

using Hackbot.Scenes;

namespace Hackbot.Structures
{
    /// <summary>
    /// Описывает ответ на сценарий для контроллера
    /// </summary>
    public class SceneResult
    {
        /// <summary>
        /// Определяет дальнешее поведение сцены.
        /// </summary>
        public enum SceneAction
        {
            /// <summary>
            /// Переход к главному меню
            /// </summary>
            Zeroize,

            /// <summary>
            /// Ожидать следующего ответа
            /// </summary>
            Continue,

            /// <summary>
            /// Переход к следующей сцене
            /// </summary>
            Next,

            /// <summary>
            /// Переход к админской панели
            /// </summary>
            Admin
        }

        /// <summary>
        /// Определяет дальнейшие действия в диалоге
        /// </summary>
        public SceneAction SceneNextAction { get; set; }

        /// <summary>
        /// Название следующей сцены. Присваивается только при SceneAction.Next
        /// </summary>
        public SceneTable NextScene { get; set; }

        /// <summary>
        /// Ответ на вопрос пользователя
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Клавиатура для пользователя
        /// </summary>
        public IReplyMarkup KeyboardMarkup { get; set; }

        /// <summary>
        /// Параметры для инициализации новой сцены
        /// </summary>
        public object[] NextSceneParams { get; set; }
    }
}
