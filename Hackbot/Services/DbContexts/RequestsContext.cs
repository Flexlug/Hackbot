using Hackbot.Structures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            => options.UseSqlite("Data Source=requests.db")
                      .EnableDetailedErrors();
    }
}
