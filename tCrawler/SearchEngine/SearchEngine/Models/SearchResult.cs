using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchEngine.Models
{
    public class SearchResult
    {
        public int TotalCount { get; set; }
        public int NumberPerPage { get; set; }
        public int CurrentPage { get; set; }
        public List<IndexedPage> Items { get; set; }
    }
}