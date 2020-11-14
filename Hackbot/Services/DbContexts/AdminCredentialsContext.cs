using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Hackbot.Structures;

using Microsoft.EntityFrameworkCore;

namespace Hackbot.Services.DbContexts
{
    /// <summary>
    /// Контекст базы данных с контактами администраторов
    /// </summary>
    public class AdminCredentialsContext : DbContext
    {
        /// <summary>
        /// Контакты администраторов
        /// </summary>
        public DbSet<Admin> Admins { get; set; }

        public AdminCredentialsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "Databases/admins.db")}")
                      .EnableDetailedErrors();
    }
}
