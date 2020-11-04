using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hackbot.Structures
{
    /// <summary>
    /// Описывает данные, необходимые для идентификации администратора
    /// </summary>
    public class Admin
    {
        /// <summary>
        /// Databse primary key
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public int UserId { get; set; }
    }
}
