using System.Data.Entity;
using EcommerceTracker.DataAccess.Contexts;

namespace EcommerceTracker
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Mappings;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Database.SetInitializer(new EcommerceTrackerInitializer());
            var db = new EcommerceTrackerContext();
            db.Database.Initialize(true);

            AutoMapperConfiguration.Configure();
        }
    }
}
