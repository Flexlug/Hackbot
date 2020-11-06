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
        /// Меню редактирования команды
        /// </summary>
        CaptainGuildEditScene,

        /// <summary>
        /// Главное меню
        /// </summary>
        MainMenu,

        /// <summary>
        /// Главное меню с функциями администратора
        /// </summary>
        MainAdminMenu,

        /// <summary>
        /// Главное меню для капитана команды
        /// </summary>
        MainCaptainMenu,

        /// <summary>
        /// Главное меню для участника команды
        /// </summary>
        MainMemberMenu,

        /// <summary>
        /// Меню регистрации команды
        /// </summary>
        RegisterGuild,

        /// <summary>
        /// Сцена отображения запросов на вступление в команду
        /// </summary>
        RequestsView,

        /// <summary>
        /// Меню поиска команды
        /// </summary>
        SearchGuild
    }
}
