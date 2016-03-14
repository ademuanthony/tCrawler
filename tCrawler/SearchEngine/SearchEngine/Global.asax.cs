using System.Timers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using SearchEngine.Crawler;

namespace SearchEngine
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            var crawler = new WebCrawler(25);
            var timer = new Timer(10000);
            timer.Elapsed += (s, e) =>
            {
                if (crawler.IsCrawling) return;
                crawler.Crawl();
            };
            timer.Start();
        }
    }
}
