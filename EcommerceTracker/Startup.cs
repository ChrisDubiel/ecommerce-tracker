using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EcommerceTracker.Startup))]
namespace EcommerceTracker
{
    using Hangfire;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("EcommerceTrackerContext");

            ConfigureAuth(app);
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
