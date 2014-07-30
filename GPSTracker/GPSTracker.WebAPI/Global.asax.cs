using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans;
using Orleans.Host.Azure.Client;
using System.Web.Mvc;
using System.Web.Routing;

namespace GPSTracker.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            if (RoleEnvironment.IsAvailable)
            {
                // running in Azure
                OrleansAzureClient.Initialize(Server.MapPath(@"~/AzureConfiguration.xml"));
            }
            else
            {
                // not running in Azure
                OrleansClient.Initialize(Server.MapPath(@"~/LocalConfiguration.xml"));
            }

            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }
}
