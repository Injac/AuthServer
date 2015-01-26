using AuthService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace AuthService.Filters
{
    public class IPHostValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var userIP = ((Microsoft.Owin.OwinContext)actionContext.Request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
            //var context = actionContext.Request.Properties["MS_OwinContext"] as System.Web.HttpContextBase;
            //string userIP = context.Request.UserHostAddress;
          
                var ok = AuthorizedIPRepository.GetAuthorizedIPs().Contains(userIP);

                if (!ok)
                {
                    actionContext.Response =
                       new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                       {
                           Content = new StringContent("Unauthorized Access To System. Your IP has been logged.")
                       };
                    return;
                }
        }
    }

}
