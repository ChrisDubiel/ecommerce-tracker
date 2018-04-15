namespace EcommerceTracker
{
    using DataAccess.Contexts;
    using EcommerceTracker.Controllers;
    using EcommerceTracker.DataAccess;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;
    using System;
    using System.Data.Entity;
    using System.Web;
    using Unity;
    using Unity.Injection;
    using Unity.Lifetime;

    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // https://stackoverflow.com/questions/23210828/asp-net-identity-2-0-unity-not-resolving-default-user-store/23245631#23245631
            // https://stackoverflow.com/questions/23810055/microsoft-practice-using-unity-register-type

            container.RegisterType<DbContext, EcommerceTrackerContext>(new HierarchicalLifetimeManager());
            container.RegisterType<UserManager<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new InjectionConstructor(new EcommerceTrackerContext()));
            container.RegisterType<AccountController>(new InjectionConstructor());

            container.RegisterType<IAuthenticationManager>(
                new InjectionFactory(
                    o => HttpContext.Current.GetOwinContext().Authentication
                )
            );
        }
    }
}