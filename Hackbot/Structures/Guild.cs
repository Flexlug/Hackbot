using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hackbot.Structures
{
    /// <summary>
    /// Описывае команду
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// Database primary key
        /// </summary>
        [Key]
        public ulong P_KEY { get; set; }

        /// <summary>
        /// Список участников
        /// </summary>
        public List<Member> Members { get; set; }

        /// <summary>
        /// Капитан команды
        /// </summary>
        public long CaptainId { get; set; }

        /// <summary>
        /// Название команды
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание (в частности кейсы, которыми они будут заниматься)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// В поиске новых сокомандников
        /// </summary>
        public bool InSearching { get; set; }
    }
}
