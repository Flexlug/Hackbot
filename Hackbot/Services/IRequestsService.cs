using Hackbot.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services
{
    /// <summary>
    /// Интерфейс, описывающий сервис заявок на вступление в команды
    /// </summary>
    public interface IRequestsService
    {
        /// <summary>
        /// Добавить в БД заявку на вступление
        /// </summary>
        /// <param name="req">Добавляемая заявка</param>
        public Task SubmitRequestAsync(Request req);

        /// <summary>
        /// Отменить заявку
        /// </summary>
        /// <param name="from">ID пользователя, который отправил заявку</param>
        /// <param name="to">ID командира команды, в которую пользователь пытался вступить</param>
        public Task RevokeRequestAsync(long from, long to);

        /// <summary>
        /// Проверить наличие заявки в БД
        /// </summary>
        /// <param name="from">ID пользователя, который отправил заявку</param>
        /// <param name="to">ID командира команды, в которую пользователь пытался вступить</param>
        /// <remarks>
        /// Данный метод имеет место быть в ситуации перед удалением из БД записи
        /// </remarks>
        /// <returns>Если true - заявка присутствует в БД и с ней можно взаимодействовать. Если false - заявка отсутсвует</returns>
        public Task<bool> ValidateRequestAsync(long from, long to);

        /// <summary>
        /// Получить все заявки на вступление в команду по ID командира команды
        /// </summary>
        /// <param name="guildId">ID командира</param>
        public Task<List<Request>> GetRequestsByCaptainIdAsync(long captainId);

        /// <summary>
        /// Получить все отправленные заявки от рассматриваемого пользователя
        /// </summary>
        /// <param name="memberId">ID пользователя</param>
        public Task<List<Request>> GetRequestsByMemberIdAsync(long memberId);

        /// <summary>
        /// Удалить все заявки на вступление по ID капитана команды
        /// </summary>
        /// <remarks>
        /// Данный метод имеет место быть при расформировании команды.
        /// </remarks>
        /// <param name="captainId">ID капитана команды</param>
        public Task RemoveRequestsByCaptainIdAsync(long captainId);

        /// <summary>
        /// Удалить все записи на вступление по ID участника
        /// </summary>
        /// <remarks>
        /// Данный метод имеет место быть при вступлении пользователя в какую-либо команду. В таком случае все раннее отправленные заявки на вступление в другие команды будут автоматически отменены. 
        /// </remarks>
        /// <param name="memberId"></param>
        public Task RemoveRequestsByMemberIdAsync(long memberId);
    }
}
