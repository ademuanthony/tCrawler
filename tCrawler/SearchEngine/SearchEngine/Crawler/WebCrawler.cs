using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web.WebPages;
using CsQuery;
using SearchEngine.Core;
using SearchEngine.Models;
using SearchEngine.POCO;

namespace SearchEngine.Crawler
{
    public class WebCrawler
    {
        private readonly IThreadCordinator _threadCordinator;
        private readonly IQueueManager<PageToCrawl> _queueManager;
        private readonly IWebRequestManager _webRequestManager;
        public WebCrawler(int maxThread = 10)
        {
            _threadCordinator = new ThreadCordinator(maxThread);
            _queueManager = new PageQueueManager();
            _webRequestManager = new WebRequestManager();
        }

        private bool _isCrawling;
        public bool IsCrawling
        {
            get
            {
                return _isCrawling;// _queueManager.Count > 0 || _threadCordinator.HasAnyRunningThread();
            }

        }

        public void Crawl()
        {
            _queueManager.Add(new PageToCrawl(new Uri("http://www.nairaland.com/programming", UriKind.Absolute))
            {
                IsBaseUrl = true,
                ParentUrl = new Uri("http://www.nairaland.com/programming", UriKind.Absolute),
                SiteName = SiteNames.NAIRALAND
            });

            _queueManager.Add(new PageToCrawl(new Uri("http://stackoverflow.com/questions", UriKind.Absolute))
            {
                IsBaseUrl = true,
                ParentUrl = new Uri("http://stackoverflow.com/questions", UriKind.Absolute),
                SiteName = SiteNames.STACKOVERFLOW
            });

            try
            {
                StartMultiThreadedCrawling();
            }
            catch (Exception e)
            {
                //ignored 
                //log the error
            }
            finally
            {
                if (_threadCordinator != null)
                    _threadCordinator.Dispose();
            }
        }

        public void StartMultiThreadedCrawling()
        {
            var crawlCompleted = false;
            _isCrawling = true;

            while (!crawlCompleted)
            {
                if (_queueManager.Count > 0)
                {
                    _threadCordinator.PerformAction(() => CrawlPage(_queueManager.GetNext()));
                }
                else if (!_threadCordinator.HasAnyRunningThread())
                {
                    crawlCompleted = true;
                    _isCrawling = false;
                }
                else
                {
                    //There are no links in the queue. Waiting for links to be scheduled...
                    Thread.Sleep(2500);
                }
            }
        }

        protected void CrawlPage(PageToCrawl page)
        {
            if (page == null) return;
            try
            {
                CQ document = _webRequestManager.MakeRequest(page.PageUrl);
                ProccessDocument(document, page);
            }
            catch (Exception exception)
            {
                //will log the error for monitoring
                _queueManager.Clear();
                _threadCordinator.CancelAll();
                throw new Exception(exception.Message, exception);
            }
        }

        protected void ProccessDocument(CQ document, PageToCrawl page)
        {
            if (page.ShouldIndex)
                switch (page.SiteName)
                {
                    case SiteNames.NAIRALAND:
                        IndexNairalandDocument(document, page);
                        break;
                    case SiteNames.STACKOVERFLOW:
                        IndexStackOverflowDocument(document, page);
                        break;
                }
            else
                switch (page.SiteName)
                {
                    case SiteNames.NAIRALAND:
                        EnqueueNairaLandDocument(document, page);
                        break;
                    case SiteNames.STACKOVERFLOW:
                        QueueStackOverflowDocument(document, page);
                        break;
                }
        }

        protected void EnqueueNairaLandDocument(CQ document, PageToCrawl page)
        {
            //enqueue pagination links
            var links = document.Select("a").Select(link => link.GetAttribute("href"))
                .Where(href => href != null && href.StartsWith("http://www.nairaland.com/programming/"))
                .Where(href => href.Length >= 38)
                .Where(href => href.Substring(37).IsInt());

            foreach (var href in links)
            {
                _queueManager.Add(new PageToCrawl(new Uri(UrlHelper.Normalize(href, "http://www.nairaland.com")))
                {
                    IsBaseUrl = false,
                    ParentUrl = page.PageUrl,
                    SiteName = SiteNames.NAIRALAND,
                    ShouldIndex = false
                });
            }

            var topicLinks =
                document.Select(".body table > tbody > tr > td > b > a").Select(link => link.GetAttribute("href")).ToList();

            foreach (var topicHref in topicLinks)
            {
                _queueManager.Add(new PageToCrawl(new Uri(UrlHelper.Normalize(topicHref, "http://www.nairaland.com")))
                {
                    IsBaseUrl = false,
                    ParentUrl = page.PageUrl,
                    ShouldIndex = true,
                    SiteName = SiteNames.NAIRALAND
                });
            }
        }

        protected void QueueStackOverflowDocument(CQ document, PageToCrawl page)
        {
            //get paginations
            var links = document.Select(".pager.fl a").Select(link => link.GetAttribute("href"));
            foreach (var href in links)
            {
                _queueManager.Add(new PageToCrawl(new Uri(UrlHelper.Normalize(href, "http://stackoverflow.com")))
                {
                    IsBaseUrl = false,
                    ParentUrl = page.PageUrl,
                    SiteName = SiteNames.STACKOVERFLOW,
                    ShouldIndex = false
                });
            }

            var topicLinks =
              document.Select("a.question-hyperlink").Select(link => link.GetAttribute("href")).ToList();

            foreach (var topicHref in topicLinks)
            {
                _queueManager.Add(new PageToCrawl(new Uri(UrlHelper.Normalize(topicHref, "http://stackoverflow.com")))
                {
                    IsBaseUrl = false,
                    ParentUrl = page.PageUrl,
                    ShouldIndex = true,
                    SiteName = SiteNames.STACKOVERFLOW
                });
            }
        }

        protected void IndexNairalandDocument(CQ document, PageToCrawl page)
        {
            if (page.PageUrl.AbsoluteUri.Contains("register") || page.PageUrl.AbsoluteUri.Contains("programming-ads")) return;
            var newIndexedPage = new IndexedPage();
            newIndexedPage.Url = page.PageUrl.AbsoluteUri;
            newIndexedPage.Title = document.Select(".body>h2").Text();
            var viewText = document.Select(".body>.bold").Text().Split('(').LastOrDefault();
            if (viewText == null) return;
            viewText = viewText.Split(' ')[0];
            if (viewText.IsInt()) newIndexedPage.ViewCount = int.Parse(viewText);
            newIndexedPage.Site = SiteNames.NAIRALAND;
            newIndexedPage.Content = document.Select(".body table div.narrow").Html();
            newIndexedPage.Author = document.Select(".body table a.user").First().Text();

            //normalize the title
            if (newIndexedPage.Title.EndsWith(" - Programming - Nairaland"))
                newIndexedPage.Title = newIndexedPage.Title.Substring(0,
                    (newIndexedPage.Title.Length - " - Programming - Nairaland".Length));

            SavePageIndex(newIndexedPage);

        }
        protected void IndexStackOverflowDocument(CQ document, PageToCrawl page)
        {
            try
            {
                var newIndexedPage = new IndexedPage();
                newIndexedPage.Url = page.PageUrl.AbsoluteUri;
                newIndexedPage.Title = document.Select("#question-header>h1").Text();
                var viewText = document.Select("#qinfo b").Skip(1).Take(1).First().InnerText;
                viewText = viewText.Split(' ')[0];
                if (viewText.IsInt()) newIndexedPage.ViewCount = int.Parse(viewText);
                newIndexedPage.Site = SiteNames.STACKOVERFLOW;
                newIndexedPage.Content = document.Select(".postcell .post-text").Text();
                newIndexedPage.Author = document.Select(".post-signature .user-info .user-details a").First().Text();

                SavePageIndex(newIndexedPage);
            }
            catch (Exception exception)
            {
                //we will log this for health monitoring
            }


        }

        private static void SavePageIndex(IndexedPage newIndexedPage)
        {
            using (var context = new DatabaseContext())
            {
                //truncate the content
                if (newIndexedPage.Content.Length > 200)
                    newIndexedPage.Content = newIndexedPage.Content.Substring(0, 200);

                if (!context.IndexedPages.Any(p => p.Url == newIndexedPage.Url))
                {
                    context.IndexedPages.Add(newIndexedPage);
                    context.SaveChanges();
                }
                else
                {
                    var indexedPage = context.IndexedPages.First(p => p.Url == newIndexedPage.Url);
                    indexedPage.ViewCount = newIndexedPage.ViewCount;
                    indexedPage.Content = newIndexedPage.Content;
                    indexedPage.Title = newIndexedPage.Title;
                    context.Entry(indexedPage).State = EntityState.Modified;
                    context.SaveChanges();
                }

            }
        }
    }
}