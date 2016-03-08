using System;
using System.Linq;
using System.Threading;

namespace SearchEngine.Core
{
    public class ThreadCordinator:IThreadCordinator
    {

        #region public members

        /// <summary>
        /// Max number of threads to use
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return _threads.Length;
            }
        }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        public void PerformAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_haveCalledAbortAll)
                throw new InvalidOperationException("Cannot call PerformAction() after CancelAll() or Dispose() have been called.");

            lock (_lock)
            {
                int freeThreadIndex = GetFreeThreadIndex();
                while (freeThreadIndex < 0)
                {
                    Thread.Sleep(100);
                    freeThreadIndex = GetFreeThreadIndex();
                }

                if (MaxThreads > 1)
                {
                    _threads[freeThreadIndex] = new Thread(new ThreadStart(action));
                    _threads[freeThreadIndex].Start();
                }
                else
                {
                    action.Invoke();
                }
            }
        }

        public void CancelAll()
        {
            //Do nothing
            _haveCalledAbortAll = true;
        }

        /// <summary>
        /// Check wheather there is any running thread
        /// </summary>
        public bool HasAnyRunningThread()
        {
            lock (_lock)
            {
                if (_threads.Any(t => t != null && t.IsAlive))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetFreeThreadIndex()
        {
            var freeThreadIndex = -1;
            var currentIndex = 0;
            lock (_lock)
            {
                foreach (var thread in _threads)
                {
                    if ((thread == null) || !thread.IsAlive)
                    {
                        freeThreadIndex = currentIndex;
                        break;
                    }

                    currentIndex++;
                }
            }
            return freeThreadIndex;
        }

        public void Dispose()
        {
            CancelAll();
        }

        #endregion

        #region private members/contr

        private readonly object _lock = new object();
        private readonly Thread[] _threads;
        bool _haveCalledAbortAll;

        public ThreadCordinator(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("MaxThreads must be from 1 to 100");
            _threads = new Thread[maxThreads];
        }
        #endregion
    }
}