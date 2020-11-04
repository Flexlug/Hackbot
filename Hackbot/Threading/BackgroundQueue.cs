using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hackbot.Threading
{
    public class BackgroundQueue
    {
        private Task previousTask = Task.FromResult(true);
        private object key = new object();

        public Task QueueTask(Action action)
        {
            lock (key)
            {
                previousTask = previousTask.ContinueWith(
                  t => action(),
                  CancellationToken.None,
                  TaskContinuationOptions.None,
                  TaskScheduler.Default);
                return previousTask;
            }
        }

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(
                  t => work(),
                  CancellationToken.None,
                  TaskContinuationOptions.None,
                  TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }

        #region Singleton

        private BackgroundQueue() { }
        private static BackgroundQueue _instance = null;

        public static BackgroundQueue GetInstance()
        {
            if (_instance == null)
                _instance = new BackgroundQueue();

            return _instance;
        }

        #endregion
    }
}
