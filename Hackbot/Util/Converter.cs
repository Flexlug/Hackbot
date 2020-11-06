using Hackbot.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hackbot.Util
{
    /// <summary>
    /// Конвертирует некоторые типы данных, представленные в данном проекте, в простые (или наоборот)
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Предпринимает попытку сконвертировать роль из строки (в частности из inline data) к GuildRoles
        /// </summary>
        /// <param name="roleName">Строковое представление роли</param>
        /// <returns></returns>
        public static GuildRoles? FromStrToGuildRole(string roleName)
        {
            switch (roleName)
            {
                case "role1":
                    return GuildRoles.Role1;
                case "role2":
                    return GuildRoles.Role2;
                case "role3":
                    return GuildRoles.Role3;
                case "role4":
                    return GuildRoles.Role4;
                case "role5":
                    return GuildRoles.Role5;
                case "OtherRole":
                    return GuildRoles.OtherRole;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Преобразовывает GuildRoles к строке
        /// </summary>
        /// <param name="guildRole">Значение структуры GuildRoles</param>
        /// <returns></returns>
        public static string GuildRoleToStr(GuildRoles guildRole)
        {
            switch (guildRole)
            {
                case GuildRoles.Role1:
                    return "Backend разработчик";
                case GuildRoles.Role2:
                    return "Frontend разработчик";
                case GuildRoles.Role3:
                    return "ГИС специалист";
                case GuildRoles.Role4:
                    return "Backend разработчик";
                case GuildRoles.Role5:
                    return "Менеджер";
                case GuildRoles.OtherRole:
                    return "Другое";

                default:
                    throw new Exception("Can't recognize guildRole in method GuildRoleToStr.");
            }
        }
    }
}
