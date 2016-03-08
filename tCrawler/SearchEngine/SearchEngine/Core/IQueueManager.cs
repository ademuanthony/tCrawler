using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchEngine.Core
{
    public interface IQueueManager<T>
    {
        /// <summary>
        /// Count of remaining items that are currently in the queue
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Queue the param
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Adds the collection of items to the queue
        /// </summary>
        void Add(IEnumerable<T> items);

        /// <summary>
        /// Gets the next item in the queue
        /// </summary>
        T GetNext();

        /// <summary>
        /// Remove all items from this queue
        /// </summary>
        void Clear();
    }
}