using AuthService.Model;
using AuthService.Security;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Handler
{
    public class AuthHandler : DelegatingHandler
    {
        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
        /// </returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";
            const string SYS_APP = "X-SYSAPP";
            const string MTOKEN = "MGMTOKEN";
            const string MSEC = "MGMSEC";


            string MGM_TOKEN = CloudConfigurationManager.GetSetting("MGMToken");
            string MGM_SECRET = CloudConfigurationManager.GetSetting("MGMSecret");


            if (request.Headers.Contains(MTOKEN) && request.Headers.Contains(MSEC))
            {

                string Token = request.Headers.GetValues(MTOKEN).First();
                string Secret = request.Headers.GetValues(MSEC).First();

                if(!(Token.Equals(MGM_TOKEN) && Secret.Equals(MGM_SECRET)))
                {
                    HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied.");
                    return await Task.FromResult(reply);
                }

            }
            else
            {


                if (request.Headers.Contains(APP_KEY) && request.Headers.Contains(APP_SECRET))
                {
                    string appKey = Uri.UnescapeDataString(request.Headers.GetValues(APP_KEY).First());
                    string appSecret = Uri.UnescapeDataString(request.Headers.GetValues(APP_SECRET).First());

                    string sysApp = string.Empty;

                    var services = new AdminServices();

                    var calledService = request.RequestUri.AbsoluteUri.Substring(request.RequestUri.AbsoluteUri.LastIndexOf("/") + 1);

                    if (!String.IsNullOrEmpty(calledService))
                    {
                        //Check, if we have a admin service here
                        if (services.AdminServiceIdentifiers.Contains(calledService))
                        {
                            if (request.Headers.Contains(SYS_APP))
                            {
                                sysApp = request.Headers.GetValues(SYS_APP).First();
                            }
                            else
                            {
                                HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied.");
                                return await Task.FromResult(reply);
                            }
                        }
                    }


                    #region CheckParameters
                    if (String.IsNullOrEmpty(appKey))
                    {
                        HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid app key.");
                        return await Task.FromResult(reply);
                    }

                    if (String.IsNullOrEmpty(appSecret))
                    {
                        HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid app secret.");
                        return await Task.FromResult(reply);
                    }
                    #endregion
                    //TODO, CREATE ANOTHER OVERLOAD
                    UserAppAuthenticationManager authManager = new UserAppAuthenticationManager();
                    bool tokenValid = false;

                    if (String.IsNullOrEmpty(sysApp))
                    {
                        tokenValid = await authManager.ValidateSystemAppToken(appKey, appSecret);
                    }
                    else
                    {
                        var mgmtSecret = CloudConfigurationManager.GetSetting("ManagementSecret");
                        if (sysApp.Equals(mgmtSecret))
                        {
                            tokenValid = await authManager.ValidateSystemAppToken(appKey, appSecret);
                        }
                        else
                        {
                            tokenValid = false;
                        }
                    }
                    if (!tokenValid)
                    {
                        HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Please check app and user data.");
                        return await Task.FromResult(reply);
                    }

                }
                else
                {
                    HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Request is missing authorization data.");
                    return await Task.FromResult(reply);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }

    }
}
