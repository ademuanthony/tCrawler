using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SearchEngine.Core
{
    public class WebRequestManager:IWebRequestManager
    {
        #region Public members

        public string MakeRequest(Uri uri)
        {
            try
            {
                var webRequest = BuildRequestObject(uri);
                var rawCotent = GetRawHtml((HttpWebResponse) webRequest.GetResponse());
                return rawCotent;
            }
            catch (WebException exception)
            {
                //ignored
            }
            return "";
        }

        #endregion

        #region protected members

        protected virtual HttpWebRequest BuildRequestObject(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            request.Accept = "*/*";

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //request.Timeout = 3000;

            return request;
        }

        protected virtual string GetRawHtml(HttpWebResponse response)
        {
            var rawHtml = "";
            try
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    rawHtml = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                //ignored
                //a logger will be added to help in system monitoring
            }

            return rawHtml;
        }

        #endregion
    }
}