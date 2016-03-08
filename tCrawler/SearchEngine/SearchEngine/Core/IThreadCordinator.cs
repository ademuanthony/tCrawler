using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Core
{
    public interface IThreadCordinator:IDisposable
    {
        /// <summary>
        /// Max number of threads to use at a time.
        /// </summary>
        int MaxThreads { get; }

        /// <summary>
        /// Will invoke the action asynchrously on a seperate thread
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        void PerformAction(Action action);

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        bool HasAnyRunningThread();

        /// <summary>
        /// Abort all running threads
        /// </summary>
        void CancelAll();
    }
}
