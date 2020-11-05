using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hackbot.Structures
{
    /// <summary>
    /// Описывает участника команды
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Primary database key
        /// </summary>
        [Key]
        public ulong P_KEY { get; set; }

        /// <summary>
        /// Имя участника, которым он/она представился
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Краткое резюме
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Роль участника в команде
        /// </summary>
        public GuildRoles Role { get; set; }

        /// <summary>
        /// ID чата для отправки уведомлений
        /// </summary>
        public long Id { get; set; }
    }
}
