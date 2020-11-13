using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Telegram.Bot.Types;

namespace Hackbot.Structures
{
    /// <summary>
    /// Обёртка над полученным от пользователя сообщением. Может являться как стандартным текстовым сообщением, так и inline событием.
    /// </summary>
    public class RecievedMessage
    {
        /// <summary>
        /// Название inline события
        /// </summary>
        public string InlineData { get; set; }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Контакт пользователя, который прислал данное сообщение
        /// </summary>
        public User From { get; set; }

        /// <summary>
        /// Информация о диалоге с пользователей
        /// </summary>
        public Chat Chat { get; set; }

        public Contact Contact { get; set; }
    }
}
