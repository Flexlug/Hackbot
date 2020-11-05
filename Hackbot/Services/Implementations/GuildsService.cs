using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Hackbot.Structures;
using Hackbot.Services.DbContexts;
using Hackbot.Threading;
using NLog;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace Hackbot.Services.Implementations
{
    /// <summary>
    /// Сервис для обращения к базе данных с командами
    /// </summary>
    public class GuildsService : IGuildsService
    {
        private GuildContext guildsDb;
        private BackgroundQueue queue;

        private Logger logger = LogManager.GetCurrentClassLogger();

        private void AddGuild(Guild guild)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    guildsDb.Guilds.Add(guild);
                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch(Exception e)
            {
                logger.Error(e, $"Error in AddGuild method. Guild: {guild?.Name} {guild?.CaptainId}");
            }
        }

        private void RemoveGuild(Guild guild)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    guildsDb.Guilds.Remove(guild);
                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in RemoveGuildAsync method. Guild: {guild?.Name} {guild?.CaptainId}");
            }
        }

        private Guild GetGuildByCaptian(long captain)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    Guild g = guildsDb.Guilds.Include(x => x.Members)
                                             .AsNoTracking()
                                             .FirstOrDefault(x => x.CaptainId == captain);
                    transaction.Commit();
                    return g;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetGuildByCaptian method. Captain: {captain}");
                return null;
            }
        }

        private List<Guild> GetGuildsByRequiredRoles(GuildRoles role)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    // Ищем команды, в которых отсутствует участник с запрашиваемой ролью
                    List<Guild> gs = guildsDb.Guilds.Include(x => x.Members)
                                                    .AsNoTracking()
                                                    .Select(x => x)
                                                    .Where(x => x.Members.Exists(mem => mem.Role != role))
                                                    .ToList();
                    transaction.Commit();
                    return gs;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetGuildsByRequiredRoles method. Role: {role}");
                return null;
            }
        }

        private bool CheckMember(long member)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    bool valid = guildsDb.Guilds.Include(x => x.Members)
                                                .SelectMany(x => x.Members)
                                                .Select(x => x.Id)
                                                .Contains(member);

                    return valid;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetAllMembers method.");
                return false;
            }
        }

        private bool CheckCaptain(long user)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    bool valid = guildsDb.Guilds.Select(x => x.CaptainId)
                                                .Contains(user);

                    transaction.Commit();
                    return valid;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in CheckCaptain method. Captain: {user}");
                return false;
            }
        }

        private List<long> GetAllMembers()
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    List<long> mems = guildsDb.Guilds.Include(x => x.Members)
                                                     .SelectMany(x => x.Members)
                                                     .Select(x => x.Id)
                                                     .ToList();

                    return mems;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetAllMembers method.");
                return null;
            }
        }

        private List<long> GetAllCaptains()
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    List<long> caps = guildsDb.Guilds.Select(x => x.CaptainId)
                                                     .ToList();

                    return caps;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetAllCaptains method.");
                return null;
            }
        }

        private List<Guild> GetNotFullGuilds()
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    // Ищем команды, в которых отсутствует участник с запрашиваемой ролью
                    List<Guild> gs = guildsDb.Guilds.Include(x => x.Members)
                                                    .AsNoTracking()
                                                    .Select(x => x)
                                                    .Where(x => x.Members.Count < 5)
                                                    .ToList();
                    transaction.Commit();
                    return gs;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetNotFullGuilds method.");
                return null;
            }
        }

        private Guild GetGuildByMember(long member)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    Guild g = guildsDb.Guilds.Include(x => x.Members)
                                             .AsNoTracking()
                                             .FirstOrDefault(x => x.Members.Exists(x => x.Id == member));
                    transaction.Commit();
                    return g;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetGuildByMember method. Member: {member}");
                return null;
            }
        }

        public async Task<List<Guild>> GetGuildsByRequiredRolesAsync(GuildRoles role) => await queue.QueueTask(() => GetGuildsByRequiredRoles(role));
        public async Task<Guild> GetGuildByCaptianAsync(long captain) => await queue.QueueTask(() => GetGuildByCaptian(captain));
        public async Task AddGuildAsync(Guild guild) => await queue.QueueTask(() => AddGuild(guild));
        public async Task RemoveGuildAsync(Guild guild) => await queue.QueueTask(() => RemoveGuild(guild));
        public async Task<bool> CheckCaptainAsync(long user) => await queue.QueueTask(() => CheckCaptain(user));
        public async Task<bool> CheckMemberAsync(long user) => await queue.QueueTask(() => CheckMember(user));
        public async Task<List<long>> GetAllMembersAsync() => await queue.QueueTask(() => GetAllMembers());
        public async Task<List<long>> GetAllCaptainsAsync() => await queue.QueueTask(() => GetAllCaptains());
        public async Task<List<Guild>> GetNotFullGuildsAsync() => await queue.QueueTask(() => GetNotFullGuilds());
        public async Task<Guild> GetGuildByMemberAsync(long member) => await queue.QueueTask(() => GetGuildByMember(member));


        #region Singleton

        private static GuildsService _instance = null;
        private GuildsService()
        {
            guildsDb = new GuildContext();
            queue = BackgroundQueue.GetInstance();
        }

        public static GuildsService GetInstance()
        {
            if (_instance == null)
                _instance = new GuildsService();

            return _instance;
        }

        #endregion
    }
}
