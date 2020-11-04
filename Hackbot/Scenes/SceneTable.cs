using System;
using System.Collections.Generic;
using System.Text;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Перечисление всех доступных сцен
    /// </summary>
    public enum SceneTable
    {
        /// <summary>
        /// Главное меню
        /// </summary>
        MainMenu,

        /// <summary>
        /// Главное меню для капитана команды
        /// </summary>
        MainCaptainMenu,

        /// <summary>
        /// Главное меню для участника команды
        /// </summary>
        MainMemberMenu,

        /// <summary>
        /// Главное меню с функциями администратора
        /// </summary>
        MainAdminMenu,

        /// <summary>
        /// Меню регистрации команды
        /// </summary>
        RegisterGuild,

        /// <summary>
        /// Меню поиска команды
        /// </summary>
        SearchGuild
    }
}
