using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hackbot.Structures
{
    /// <summary>
    /// Описывает структуру заявки на вступление в команду
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Primary database key
        /// </summary>
        [Key]
        public ulong P_KEY { get; set; }

        /// <summary>
        /// ID человека, от которого поступила заявкуа на вступление
        /// </summary>
        public long From { get; set; }

        /// <summary>
        /// ID капитана гильдии, в которую человек хочет вступить
        /// </summary>
        public long To { get; set; }

        /// <summary>
        /// Роль, на которую хочет попасть человек
        /// </summary>
        public GuildRoles RequestingRole { get; set; }

        /// <summary>
        /// Имя человека, которым он представился
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Краткое резюме
        /// </summary>
        public string Description { get; set; }
    }
}
