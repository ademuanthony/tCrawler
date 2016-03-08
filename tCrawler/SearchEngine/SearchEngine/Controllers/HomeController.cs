using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsQuery.ExtensionMethods;
using SearchEngine.Models;

namespace SearchEngine.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Search(string q, int page = 1)
        {
            try
            {
                const int resultsPerPage = 10;
                using (var context = new DatabaseContext())
                {
                    var reangedText = q;
                    var termsSplitted = q.Split(' ');
                    if (termsSplitted.Length > 1)
                    {
                        reangedText = "";
                        for (var i = 0; i < termsSplitted.Length; i++)
                        {
                            reangedText += string.Format("\"{0}\"", termsSplitted[i]);
                            if (i < termsSplitted.Length - 1) reangedText += "AND";
                        }
                    }
                    var ftsQuery = FtsInterceptor.Fts(reangedText);
                    var resultQuery = context.IndexedPages.Where(p => p.Title.Contains(ftsQuery));

                    var skip = resultsPerPage * (page - 1);
                    var count = resultQuery.Count();
                    var items = resultQuery.OrderBy(r=>r.ReplyCount).Skip(skip).Take(resultsPerPage).ToList();
                    return
                        Json(new SearchResult
                        {
                            CurrentPage = page,
                            Items = items,
                            NumberPerPage = resultsPerPage,
                            TotalCount = count
                        }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception exception)
            {
                return Json(new SearchResult());
            }
           
        }
    }
}