using System;
using System.Data.Entity; // Required for Database.SetInitializer
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Web.Optimization;

namespace OnlineBookstore
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // ✅ Disable EF initializer — you manage the DB manually
            Database.SetInitializer<Models.OnlineBookstoreContext>(null);

            // ✅ Optional: Just test connection (no Initialize)
            using (var context = new Models.OnlineBookstoreContext())
            {
                var _ = context.Books.Count(); // Forces connection, but no schema check
            }
        }
    }
}