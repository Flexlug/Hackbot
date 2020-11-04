using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Hackbot.Structures;

namespace Hackbot.Services.DbContexts
{
    /// <summary>
    /// Контекс базы данных со всеми зарегистрированными командами
    /// </summary>
    public class GuildContext : DbContext
    {
        /// <summary>
        /// Зарегистрированные команды
        /// </summary>
        public DbSet<Guild> Guilds { get; set; }

        public GuildContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=guilds.db")
                      .EnableDetailedErrors();
    }
}
