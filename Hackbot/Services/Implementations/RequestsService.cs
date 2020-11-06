using Hackbot.Services.DbContexts;
using Hackbot.Structures;
using Hackbot.Threading;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Services.Implementations
{
    /// <summary>
    /// Cервис заявок на вступление в команды
    /// </summary>
    public class RequestsService : IRequestsService
    {
        private RequestsContext requestsDb;
        private BackgroundQueue queue;

        private Logger logger = LogManager.GetCurrentClassLogger();

        private void SubmitRequest(Request req)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    requestsDb.Requests.Add(req);
                    transaction.Commit();
                }
                requestsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in SubmitRequest method. Request: {req?.From} {req?.To} {req?.Name}");
            }
        }

        private void RevokeRequest(long from, long to)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    Request req = requestsDb.Requests.First(x => x.From == from && x.To == to);
                    requestsDb.Requests.Remove(req);

                    transaction.Commit();
                }
                requestsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in RevokeRequest method. from: {from}, to: {to}");
            }
        }

        private bool ValidateRequest(long from, long to)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    Request req = requestsDb.Requests.FirstOrDefault(x => x.From == from && x.To == to);
                    transaction.Commit();

                    return req != null ? true : false;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in ValidateRequest method. from: {from}, to: {to}");
                return false;
            }
        }

        private List<Request> GetRequestsByCaptainId(long captainId)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    List<Request> reqs = requestsDb.Requests.AsNoTracking()
                                                            .Select(x => x)
                                                            .Where(x => x.To == captainId)
                                                            .ToList();
                    transaction.Commit();

                    return reqs;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetRequestsByCaptainId method. captainId: {captainId}");
                return null;
            }
        }

        private List<Request> GetRequestsByMemberId(long memberId)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    List<Request> reqs = requestsDb.Requests.AsNoTracking()
                                                            .Select(x => x)
                                                            .Where(x => x.From == memberId)
                                                            .ToList();
                    transaction.Commit();

                    return reqs;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in GetRequestsByMemberId method. memberId: {memberId}");
                return null;
            }
        }

        private void RemoveRequestsByCaptainId(long captainId)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    List<Request> reqs = requestsDb.Requests.Select(x => x)
                                                            .Where(x => x.To == captainId)
                                                            .ToList();
                    requestsDb.Requests.RemoveRange(reqs);
                    transaction.Commit();
                }
                requestsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in RemoveRequestsByCaptainId method. captainId: {captainId}");
            }
        }

        private void RemoveRequestsByMemberId(long memberId)
        {
            try
            {
                using (var transaction = requestsDb.Database.BeginTransaction())
                {
                    List<Request> reqs = requestsDb.Requests.Select(x => x)
                                                            .Where(x => x.From == memberId)
                                                            .ToList();
                    requestsDb.Requests.RemoveRange(reqs);
                    transaction.Commit();
                }
                requestsDb.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in RemoveRequestsByMemberId method. memberId: {memberId}");
            }
        }

        public async Task SubmitRequestAsync(Request req) => await queue.QueueTask(() => SubmitRequest(req));
        public async Task RevokeRequestAsync(long from, long to) => await queue.QueueTask(() => RevokeRequest(from, to));
        public async Task<bool> ValidateRequestAsync(long from, long to) => await queue.QueueTask(() => ValidateRequest(from, to));
        public async Task<List<Request>> GetRequestsByCaptainIdAsync(long captainId) => await queue.QueueTask(() => GetRequestsByCaptainId(captainId));
        public async Task<List<Request>> GetRequestsByMemberIdAsync(long memberId) => await queue.QueueTask(() => GetRequestsByMemberId(memberId));
        public async Task RemoveRequestsByCaptainIdAsync(long captainId) => await queue.QueueTask(() => RemoveRequestsByCaptainId(captainId));
        public async Task RemoveRequestsByMemberIdAsync(long memberId) => await queue.QueueTask(() => RemoveRequestsByMemberId(memberId));

        #region Singleton

        private static RequestsService _instance = null;
        private RequestsService()
        {
            requestsDb = new RequestsContext();
            queue = new BackgroundQueue();
        }

        public static RequestsService GetInstance()
        {
            if (_instance == null)
                _instance = new RequestsService();

            return _instance;
        }

        #endregion
    }
}
