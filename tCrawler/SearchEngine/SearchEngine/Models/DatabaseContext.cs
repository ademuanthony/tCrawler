using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;

namespace SearchEngine.Models
{
    public class DatabaseContext:DbContext
    {
        public DatabaseContext():base(nameOrConnectionString : "DatabaseContext")
        {
            DbInterception.Add(new FtsInterceptor());
        }
        public IDbSet<IndexedPage> IndexedPages { get; set; } 
    }
}