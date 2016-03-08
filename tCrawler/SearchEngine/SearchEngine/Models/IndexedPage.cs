using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchEngine.Models
{
    public class IndexedPage
    {
        public IndexedPage()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Site { get; set; }
        public string Url { get; set; }
        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }
        public string Author { get; set; }
    }
}