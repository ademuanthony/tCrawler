using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchEngine.POCO
{
    public class PageToCrawl
    {
        public bool IsBaseUrl { get; set; }

        public Uri PageUrl { get; set; }

        public Uri ParentUrl { get; set; }

        public string SiteName { get; set; }

        public bool ShouldIndex { get; set; }
         
        public override string ToString()
        {
            return PageUrl.AbsoluteUri;
        }

        public PageToCrawl(Uri pageUrl)
        {
            if (pageUrl == null) throw new ArgumentNullException("pageUrl", "pageUrl cannot be null");
            PageUrl = pageUrl;
        }
    }
}