using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchEngine.Core
{
    public class UrlHelper
    {
        public static string Normalize(string url, string baseUrl)
        {
            if (url.ToLower().StartsWith("http")) return url;
            if (!baseUrl.StartsWith("http")) baseUrl = "http://" + baseUrl;
            if (baseUrl.EndsWith("/")) baseUrl = baseUrl.Remove(baseUrl.Length - 1, 1);
            if (url.StartsWith("/")) url = url.Remove(0, 1);
            return string.Format("{0}/{1}", baseUrl, url);
        }
    }
}