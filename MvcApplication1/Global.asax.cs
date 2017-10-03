using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using Hangfire.Common;
using Newtonsoft.Json;
using Hangfire.MemoryStorage;
using MvcApplication1.Models;

namespace MvcApplication1
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {

        private BackgroundJobServer server;

        protected void Application_Start()
        {

            JsonSerializerSettings jsSettings = new JsonSerializerSettings();
            jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            
            AreaRegistration.RegisterAllAreas();
            
            Hangfire.GlobalConfiguration.Configuration.UseMemoryStorage();

            RecurringJob.AddOrUpdate(() => Readiness.MinutelyTaskChecker(), Cron.Minutely());
            RecurringJob.AddOrUpdate(() => Readiness.TerminationCheck(), Cron.Minutely());

            server = new BackgroundJobServer();

            Log.AppendBlankLine();

            Global.LoadSettings();
            Global.SaveSettings();
            Global.ImportEmailFile();

            //PSTImporter.SyncPSTFiles();
            //Global.GetAllEmails();
             
            // Web API routes
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            Log.Append("**********************************Application terminated**********************************");
            Log.Append("**********************************Application terminated**********************************");
            Log.Append("**********************************Application terminated**********************************");
            Log.Append("**********************************Application terminated**********************************");
        }

        public static class WebApiConfig
        {
            public static void Register(HttpConfiguration config)
            {
                // Web API routes
                config.MapHttpAttributeRoutes();
                
                // Convention-based routing.
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            }
        }
    }
}