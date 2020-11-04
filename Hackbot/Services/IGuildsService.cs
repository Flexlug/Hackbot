using Hackbot.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    /// <summary>
    /// Предоставляет доступ к базе данных с командами
    /// </summary>
    public interface IGuildsService
    {
        /// <summary>
        /// Добавить в БД запись о новой команде
        /// </summary>
        /// <param name="guild">Ссылка на команду</param>
        public Task AddGuildAsync(Guild guild);

        /// <summary>
        /// Удалить команду из БД
        /// </summary>
        /// <param name="guild">Ссылка на команду</param>
        /// <returns></returns>
        public Task RemoveGuildAsync(Guild guild);

        /// <summary>
        /// Получить список команд, в которых отсутствует определённая роль
        /// </summary>
        /// <param name="role">Запрашиваемая роль</param>
        /// <returns></returns>
        public Task<List<Guild>> GetGuildsByRequiredRolesAsync(GuildRoles role);

        /// <summary>
        /// Получить список неполных команд
        /// </summary>
        /// <returns></returns>
        public Task<List<Guild>> GetNotFullGuildsAsync();

        /// <summary>
        /// Получить команду по данным о капитане
        /// </summary>
        /// <param name="captain">ID капитана</param>
        /// <returns></returns>
        public Task<Guild> GetGuildByCaptianAsync(long captain);

        /// <summary>
        /// Получить команду по данным об участнике
        /// </summary>
        /// <param name="member">ID участника</param>
        /// <returns></returns>
        public Task<Guild> GetGuildByMemberAsync(long member);

        /// <summary>
        /// Проверить, явлется ли пользователь командиром какой-нибудь команды
        /// </summary>
        /// <param name="user">ID пользователя</param>
        /// <returns></returns>
        public Task<bool> CheckCaptainAsync(long user);

        /// <summary>
        /// Проверить, является ли пользователь участником какой-нибудь команды
        /// </summary>
        /// <param name="user">ID пользователя</param>
        /// <returns></returns>
        public Task<bool> CheckMemberAsync(long user);

        /// <summary>
        /// Получить список из всех, кто зарегистрирован в качестве участника
        /// </summary>
        /// <returns></returns>
        public Task<List<long>> GetAllMembersAsync();

        /// <summary>
        /// Получить список всех командиров
        /// </summary>
        /// <returns></returns>
        public Task<List<long>> GetAllCaptainsAsync();
    }
}
