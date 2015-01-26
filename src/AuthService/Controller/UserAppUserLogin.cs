using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AuthService.Model;
using AuthService.Security;
using System.Net;
using Newtonsoft.Json;

namespace AuthService.Controller
{
    /// <summary>
    /// User app user login.
    /// </summary>
    public class UserAppLoginController : ApiController
    {
        private const string LogName = "APPUSERLOGIN";

        /// <summary>
        /// Logins the user app user.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> LoginUser(dynamic data)
        {
            Logging.Logger logger = new Logging.Logger(LogName);

            //Generate authentication data
            UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();
            using (var userApps = new userappsEntities())
            {

                userApps.ChangeTracker.DetectChanges();
                try
                {

                    int appId = data.appId;
                    string password = data.password;
                    string userName = data.userName;

                    #region checkParameters
                    if (appId <= 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Application id cannot be 0 or negative.");
                    }
                    if (string.IsNullOrEmpty(userName))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username cannot be null or empty.");
                    }
                    if (string.IsNullOrEmpty(password))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Password cannot be null or empty.");
                    }
                    #endregion

                    var userAppUser = userApps.users.Where(uau => uau.username.ToLower().Equals(userName.ToLower()) && uau.appid == appId).FirstOrDefault();
                    if (userAppUser != null)
                    {
                        
                       // var userHashValue = String.Format("{0}{1}",userName,DateTime.Now.ToLongDateString());

                    
                        var pwdMatch =  authManger.DoesPasswordMatch(userAppUser.password, password);

                        if (pwdMatch)
                        {
                            await logger.StoreNewLogMessage(new Logging.Message(String.Format("UAPPLOGINSUCCESS for user {0}."
                                 , userName), LogName));

                            var Message = new LoginStatus(){ Message = "SUCCESS"};


                            return Request.CreateResponse(HttpStatusCode.OK, Message, Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            await logger.StoreNewLogMessage(new Logging.Message(String.Format("UAPPLOGINERRO for user {0} Message: {1}."
                                , userName, "Wrong login data."), LogName));

                            var Message = new LoginStatus() { Message = "FAILURE" };

                            return Request.CreateResponse(HttpStatusCode.OK, Message, Configuration.Formatters.JsonFormatter);
                        }

                    }
                    else
                    {
                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("UAPPLOGINERRO for user {0} Message: {1}."
                               , userName, "No such app user."), LogName));
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not exist.");
                    }

                                     

                }
                catch (Exception ex)
                {
                    logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                  , "LoginUserApp", ex.ToString()), LogName)).Wait();
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
                }
                
            }

        }


        private class LoginStatus
        {
            public string Message { get; set; }
        }
    }
}