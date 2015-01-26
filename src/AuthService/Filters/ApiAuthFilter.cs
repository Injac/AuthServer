using AuthService.Model;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace AuthService.Filters
{
    /// <summary>
    /// Checking for the correct
    /// management token.
    /// </summary>
    public class ApiAuthFilter : AuthorizationFilterAttribute
    {

        const string MTOKEN = "MGMTOKEN";
        const string MSEC = "MGMSEC";
        const string SYS_APP = "X-SYSAPP";


        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute" />.</param>
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {

            var header = string.Empty;

           
            if (actionContext.Request.Headers.Contains(SYS_APP))

            {
                header = actionContext.Request.Headers.GetValues(SYS_APP).First();
            }

            var keyValue = CloudConfigurationManager.GetSetting("ManagementSecret");
            string MGM_TOKEN = CloudConfigurationManager.GetSetting("MGMToken");
            string MGM_SECRET = CloudConfigurationManager.GetSetting("MGMSecret");

            string Token = actionContext.Request.Headers.GetValues(MTOKEN).First();
            string Secret = actionContext.Request.Headers.GetValues(MSEC).First();

            if(string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Secret))
            {
                var response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied.");
                actionContext.Response = response;  
            }

            if(!keyValue.Equals(header))
            {
                var response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied.");
                actionContext.Response = response;  
            }

            if (!(Token.Equals(MGM_TOKEN) && Secret.Equals(MGM_SECRET)))
            {
                var response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied.");
                actionContext.Response = response;  
            }
           
                      

            base.OnAuthorization(actionContext);
        }
    }
}
