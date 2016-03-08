using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.POCO;

namespace SearchEngine.Core
{
    public class PageQueueManager:IQueueManager<PageToCrawl>
    {

        #region public members
        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// </summary>
        public int Count
        {
            get
            {
                return _pages.Count;
            }
        }

        /// <summary>
        /// Schedules the param to be crawled in a FIFO fashion
        /// </summary>
        public void Add(PageToCrawl page)
        {
            if (page == null)
                throw new ArgumentNullException("page", "page can not be null");

            if (_scheduledOrCrawledUrls.TryAdd(page.PageUrl.AbsoluteUri, null))
            {
                _pages.Enqueue(page);
            }
        }

        public void Add(IEnumerable<PageToCrawl> pages)
        {
            if (pages == null)
                throw new ArgumentNullException("pages", "pages cannot be null");

            foreach (var page in pages)
                Add(page);
        }

        /// <summary>
        /// Gets the next page to crawl from the queue
        /// </summary>
        public PageToCrawl GetNext()
        {
            PageToCrawl nextItem = null;

            if (_pages.Count > 0)
                _pages.TryDequeue(out nextItem);

            return nextItem;
        }

        /// <summary>
        /// Clear all currently qeueud pages
        /// </summary>
        public void Clear()
        {
            _pages = new ConcurrentQueue<PageToCrawl>();
        }

        #endregion

        #region private members
        private ConcurrentQueue<PageToCrawl> _pages = new ConcurrentQueue<PageToCrawl>();
        private readonly ConcurrentDictionary<string, object> _scheduledOrCrawledUrls = new ConcurrentDictionary<string, object>();
        #endregion
    }
}