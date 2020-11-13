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

        private IRequestsService requests;

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
                    Guild deleteingGuild = guildsDb.Guilds.Include(x => x.Members)
                                                          .FirstOrDefault(x => x.CaptainId == guild.CaptainId);
                    deleteingGuild.Members.Clear();

                    guildsDb.Guilds.Remove(deleteingGuild);
                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in RemoveGuildAsync method. Guild: {guild?.Name} {guild?.CaptainId}");
            }
        }

        private void AddMemberToGuild(Guild guild, Member member)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    Guild dbGuild = guildsDb.Guilds.Include(x => x.Members)
                                                   .First(x => x.CaptainId == guild.CaptainId);
                    dbGuild.Members.Add(member);

                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in AddMemberToGuild method. Guild: {guild?.Name} {guild?.CaptainId}. Member: {member.Id}");
            }
        }

        private void RemoveMemberFromGuild(Guild guild, Member member)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    Guild dbGuild = guildsDb.Guilds.Include(x => x.Members)
                                                   .First(x => x.CaptainId == guild.CaptainId);
                    Member deletingMember = dbGuild.Members.First(x => x.Id == member.Id);
                    dbGuild.Members.Remove(deletingMember);

                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in AddMemberToGuild method. Guild: {guild?.Name} {guild?.CaptainId}. Member: {member.Id}");
            }
        }

        private void ChangeGuildDescription(Guild guild, string description)
        {
            try
            {
                using (var transaction = guildsDb.Database.BeginTransaction())
                {
                    Guild g = guildsDb.Guilds.FirstOrDefault(x => x.CaptainId == guild.CaptainId);
                    g.Description = description;

                    transaction.Commit();
                }
                guildsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in ChangeGuildDescription method. GuildId: {guild.CaptainId}. Description: {description}");
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

        private List<Guild> GetAvaliableGuilds(long memberId)
        {
            List<Request> Rawreqs = requests.GetRequestsByMemberIdAsync(memberId).Result;

            List<long> reqs = Rawreqs?.Select(x => x.To)
                                      .ToList();

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

                    if (gs == null)
                        return null;

                    // Проверяем, есть ли в списке команды, в которые была подана заявка и удаляем таковые из списка
                    foreach (Guild g in gs)
                        if (reqs.Contains(g.CaptainId))
                            gs.Remove(g);

                    return gs;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetAvaliableGuilds method. memberId: {memberId}");
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
                    List<Guild> gs = guildsDb.Guilds.Include(x => x.Members)
                                                    .AsNoTracking()
                                                    .Select(x => x)
                                                    .ToList();
                                             
                    Guild g = gs.FirstOrDefault(x => x.Members.Exists(x => x.Id == member));
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

        public async Task<List<Guild>> GetAvaliableGuildsAsync(long memberId) => await queue.QueueTask(() => GetAvaliableGuilds(memberId));
        public async Task<Guild> GetGuildByCaptianAsync(long captain) => await queue.QueueTask(() => GetGuildByCaptian(captain));
        public async Task AddGuildAsync(Guild guild) => await queue.QueueTask(() => AddGuild(guild));
        public async Task RemoveGuildAsync(Guild guild) => await queue.QueueTask(() => RemoveGuild(guild));
        public async Task<bool> CheckCaptainAsync(long user) => await queue.QueueTask(() => CheckCaptain(user));
        public async Task<bool> CheckMemberAsync(long user) => await queue.QueueTask(() => CheckMember(user));
        public async Task<List<long>> GetAllMembersAsync() => await queue.QueueTask(() => GetAllMembers());
        public async Task<List<long>> GetAllCaptainsAsync() => await queue.QueueTask(() => GetAllCaptains());
        public async Task<Guild> GetGuildByMemberAsync(long member) => await queue.QueueTask(() => GetGuildByMember(member));
        public async Task AddMemberToGuildAsync(Guild guild, Member member) => await queue.QueueTask(() => AddMemberToGuild(guild, member));
        public async Task RemoveMemberFromGuildAsync(Guild guild, Member member) => await queue.QueueTask(() => RemoveMemberFromGuild(guild, member));
        public async Task ChangeGuildDescriptionAsync(Guild guild, string description) => await queue.QueueTask(() => ChangeGuildDescription(guild, description));


        #region Singleton

        private static GuildsService _instance = null;
        private GuildsService()
        {
            guildsDb = new GuildContext();
            queue = new BackgroundQueue();

            requests = RequestsService.GetInstance();
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
