using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Hackbot.Structures;

namespace Hackbot.Services.DbContexts
{
    /// <summary>
    /// Контекст базы данных со всеми зарегистрированными командами
    /// </summary>
    public class RequestsContext : DbContext
    {
        /// <summary>
        /// Активные заявки
        /// </summary>
        public DbSet<Request> Requests { get; set; }

        public RequestsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "requests.db")}")
                      .EnableDetailedErrors();
    }
}
