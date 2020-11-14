using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Hackbot.Structures;

namespace Hackbot.Services.DbContexts
{
    /// <summary>
    /// Контекст базы данных со всеми зарегистрированными командами и участниками на хакатон
    /// </summary>
    public class GuildsContext : DbContext
    {
        /// <summary>
        /// Зарегистрированные команды
        /// </summary>
        public DbSet<Guild> Guilds { get; set; }

        /// <summary>
        /// Зарегистрированные участники
        /// </summary>
        public DbSet<Member> Members { get; set; }

        public GuildsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "Databases/guilds.db")}")
                      .EnableDetailedErrors();
    }
}
