using AuthService.Filters;
using AuthService.Handler;
using AuthService.Model;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using WebApiThrottle;

namespace AuthService.OwinStartup
{
    /// <summary>
    /// Class that is responsible to fire
    /// up the owin host.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { id = RouteParameter.Optional }
            );

            //ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            //builder.EntitySet<promotioncode>("promotioncodes");

            //config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());


            config.MessageHandlers.Add(new AuthHandler());
            //config.Filters.Add(new IPHostValidationAttribute());
            config.EnableCors();

            //Throttling settings for the web api
            //Only whitelisted client is the appadditives site
            config.MessageHandlers.Add(new ThrottlingHandler()
            {
                Policy = new ThrottlePolicy(perSecond: 30, perMinute: 500, perHour: 7200)
                {
                    IpThrottling = true,
                    #if DEBUG
                    IpWhitelist = new List<string> { "127.0.0.1-127.0.0.1"}
                    #else
                    //ADD YOUR IP'S here
                    IpWhitelist = new List<string> { "138.91.171.145-138.91.171.145","23.97.212.117-23.97.212.117" }
                    #endif
                },
                Repository = new AzureCacheThrottleRepository()
            });



            app.UseWebApi(config);
        }
    }

}
