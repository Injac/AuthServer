using AuthService.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using AuthService.Model;
using AuthService.Filters;
using Newtonsoft.Json;
using System.Web.Http.Cors;

namespace AuthService.Controller
{
    /// <summary>
    /// Control user and system apps.
    /// </summary>
    [EnableCors(origins: "[YOUR ORIGIN]", headers: "*", methods: "*")]
    public class AppManagmentController : ApiController
    {

        public class IncomingData
        {
            public int systemuserid
            {
                get;
                set;
            }
            public int appid
            {
                get;
                set;
            }
        }

        #region SystemApps
        /// <summary>
        /// Adds the system application user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sysappid">The sysappid.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">You have to pass a systemapp id.;sysappid
        /// or
        /// You have to pass a a valid username;userId</exception>
        [HttpPost]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> AddSystemAppUser(IncomingData appData)
        {


            int userId = appData.systemuserid;
            int sysappid = appData.appid;

            if (userId == 0 || userId <= 0)
            {
                throw new ArgumentException("You have to pass a systemapp id.", "sysappid");
            }

            if (sysappid == null || sysappid <= 0)
            {
                throw new ArgumentException("You have to pass a a valid username", "userId");
            }

            using (var userapps = new Model.userappsEntities())
            {
                using (var system = new Model.exgripEntities())
                {
                    if (!userapps.systemapps.Any(a => a.id == sysappid))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                           "System application does not exist");
                    }

                    else
                    {
                        userapps.ChangeTracker.DetectChanges();
                        var systemUser = system.UserProfiles.Where(sus => sus.UserId == userId).FirstOrDefault();
                        var sysAppUser = userapps.systemappusers.Where(us => us.systemuserid == systemUser.UserId).FirstOrDefault();
                        Model.systemappuser sysAppUserEntry = null;

                        if (sysAppUser == null)
                        {
                            try
                            {
                                var password = Membership.GeneratePassword(10, 3);
                                //Generate authentication data
                                UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();
                                var user = await authManger.IssueTokenSysApp(systemUser.UserName, password, systemUser.UserId, sysappid);
                                sysAppUserEntry = new Model.systemappuser()
                                {
                                    systemuserid = systemUser.UserId,
                                    appSecret = user.Secret,
                                    apptoken = user.Token,
                                    appid = sysappid,
                                    securitySoup = user.SecSoup
                                };
                                userapps.systemappusers.Add(sysAppUserEntry);
                                await userapps.SaveChangesAsync();
                            }

                            catch (Exception ex)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                                   String.Format("Database error. Exception:{1}", ex.Message));
                            }

                            return Request.CreateResponse<Model.systemappuser>(sysAppUserEntry);
                        }

                        else
                        {
                            try
                            {
                                var password = Membership.GeneratePassword(10, 3);
                                //Generate authentication data
                                UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();
                                var user = await authManger.IssueTokenSysApp(systemUser.UserName, password, systemUser.UserId, sysappid);


                                sysAppUser.appSecret = user.Secret;
                                sysAppUser.apptoken = user.Token;
                                sysAppUser.securitySoup = user.SecSoup;


                                await userapps.SaveChangesAsync();
                            }

                            catch (Exception ex)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                                   String.Format("Database error. Exception:{1}", ex.Message));
                            }

                            return Request.CreateResponse<Model.systemappuser>(sysAppUser);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the system application user API keys.
        /// </summary>
        /// <param name="appData">The application data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// You have to pass a systemapp id.;sysappid
        /// or
        /// You have to pass a a valid username;userId
        /// </exception>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<systemappuser> GetSystemAppUserApiKeys(IncomingData appData)
        {

            int userId = appData.systemuserid;
            int sysappid = appData.appid;

            if (userId == 0 || userId <= 0)
            {
                throw new ArgumentException("You have to pass a systemapp id.", "sysappid");
            }

            if (sysappid == null || sysappid <= 0)
            {
                throw new ArgumentException("You have to pass a a valid username", "userId");
            }

            return await Task.Run<systemappuser>(() =>
            {
                try
                {
                    using (var systemapps = new userappsEntities())
                    {
                        using (var system = new Model.exgripEntities())
                        {

                            var user = system.UserProfiles.Where(u => u.UserId == userId).FirstOrDefault();

                            var sysApp = systemapps.systemapps.Where(app => app.id == sysappid).FirstOrDefault();

                            if ((user != null) && (sysApp != null))
                            {

                                var systemAppUser = systemapps.systemappusers.Where(sysusr => sysusr.appid == sysappid && sysusr.systemuserid == userId).FirstOrDefault();

                                if (systemAppUser != null)
                                {
                                    return new systemappuser()
                                    {
                                        apptoken = systemAppUser.apptoken, appSecret = systemAppUser.appSecret
                                    };
                                }

                                else
                                {
                                    return null;
                                }
                            }

                            else
                            {
                                return null;
                            }

                        }

                    }
                }

                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

            });
        }

        /// <summary>
        /// Deletes the system application user.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// You have to pass a systemapp object.;app
        /// or
        /// You have to pass a a valid username;userName
        /// </exception>
        [HttpPost]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> DeleteSystemAppUser(dynamic data)
        {
            if (data.appName == null)
            {
                throw new ArgumentException("You have to pass a systemapp object.", "app");
            }

            if (data.userName == null)
            {
                throw new ArgumentException("You have to pass a a valid username", "userName");
            }

            string userName = data.userName;
            string appname = data.appName;

            using (var userapps = new Model.userappsEntities())
            {
                using (var system = new Model.exgripEntities())
                {
                    if (!userapps.systemapps.Any(a => a.appname.ToLower().
                                                 Equals(appname.ToLower())))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                           "System application does not exist");
                    }

                    else
                    {
                        var sysAppUser = system.UserProfiles.Where(sus =>
                                         sus.UserName.ToLower().Equals(userName.ToLower())).FirstOrDefault();
                        var sysApp = userapps.systemapps.Where(a => a.appname.ToLower().
                                                               Equals(appname.ToLower())).FirstOrDefault();

                        if (sysAppUser == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "System user does not exist");
                        }

                        else
                        {
                            Model.systemappuser sysUserInApp = null;

                            try
                            {
                                userapps.ChangeTracker.DetectChanges();
                                sysUserInApp = userapps.systemappusers.Where(
                                                   sa => sa.appid == sysApp.id && sa.systemuserid == sysAppUser.UserId).FirstOrDefault();

                                if (sysUserInApp == null)
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                                       "System app user cannot be found.");
                                }

                                userapps.systemappusers.Remove(sysUserInApp);
                                await userapps.SaveChangesAsync();
                            }

                            catch (Exception ex)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                                   String.Format("Database error. Exception:{0}", ex.Message));
                            }

                            return Request.CreateResponse<Model.systemappuser>(sysUserInApp);
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Deletes the system application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">You have to pass a systemapp object.;app</exception>
        [HttpPost]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> DeleteSystemApp([FromBody] string appName)
        {
            if (String.IsNullOrEmpty(appName))
            {
                throw new ArgumentException("You have to pass a systemapp object.", "app");
            }

            using (var userapps = new Model.userappsEntities())
            {
                if (!userapps.systemapps.Any(a => a.appname.ToLower().
                                             Equals(appName.ToLower())))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "System app does not exist");
                }

                else
                {
                    try
                    {
                        userapps.ChangeTracker.DetectChanges();
                        var sysApp = userapps.systemapps.Where(sa => sa.appname.ToLower().Equals(
                                appName.ToLower())).FirstOrDefault();
                        userapps.systemapps.Remove(sysApp);
                        await userapps.SaveChangesAsync();
                    }

                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                           String.Format("Database error. Exception:{1}", ex.Message));
                    }

                    return Request.CreateResponse<string>(appName);
                }
            }
        }


        /// <summary>
        /// Creates the system application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">You have to pass a systemapp object.;app</exception>
        [HttpPost]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> CreateSystemApp(dynamic appdata)
        {

            Model.systemapp app = new systemapp();

            app = await JsonConvert.DeserializeObjectAsync<systemapp>(appdata);



            if (app == null)
            {
                throw new ArgumentException("You have to pass a systemapp object.", "app");
            }

            using (var userapps = new Model.userappsEntities())
            {
                if (userapps.systemapps.Any(a => a.appname.ToLower().
                                            Equals(app.appname.ToLower())))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "System app already exists");
                }

                else
                {
                    try
                    {
                        userapps.ChangeTracker.DetectChanges();
                        userapps.systemapps.Add(app);
                        await userapps.SaveChangesAsync();
                    }

                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                           String.Format("Database error. Exception:{1}", ex.Message));
                    }

                    return Request.CreateResponse<Model.systemapp>(app);
                }
            }
        }

        /// <summary>
        /// Lists all system apps.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public HttpResponseMessage ListAllSystemApps()
        {
            using (var userApps = new userappsEntities())
            {
                var sysApps = userApps.systemapps.ToList();

                if (sysApps != null)
                {
                    if (sysApps.Count > 0)
                    {
                        return Request.CreateResponse<List<systemapp>>(sysApps);
                    }

                }

                return Request.CreateResponse(HttpStatusCode.OK, "No system apps could be found.");

            }
        }

        /// <summary>
        /// Gets the system application.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> ListSystemApp(dynamic appValue)
        {
            var appName = (string)appValue;
            return await Task<HttpResponseMessage>.Run(() =>
            {

                using (var userApps = new userappsEntities())
                {
                    var sysApp = userApps.systemapps.Where(a => a.appname.ToLower().Equals(appName.ToLower())).FirstOrDefault();

                    if (sysApp != null)
                    {
                        return Request.CreateResponse<systemapp>(sysApp);
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No such system app available.");
                    }
                }
            });

        }


        #endregion

        #region UserApps
        /// <summary>
        /// Deletes the user application.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Application id cannot be 0 or negative.;appId</exception>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> DeleteUserApp(int appId)
        {
            if (appId <= 0)
            {
                throw new ArgumentException("Application id cannot be 0 or negative.", "appId");
            }

            using (var userapps = new userappsEntities())
            {
                userapps.ChangeTracker.DetectChanges();
                var userApp = userapps.apps.Where(app => app.idapps == appId).FirstOrDefault();

                if (userApp == null)
                {
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Application id is invalid.");
                }

                else
                {
                    //First delete all connections between apps and app users
                    var appUsers = userapps.users.Where(aus => aus.appid == userApp.idapps);

                    try
                    {
                        if (appUsers != null)
                        {
                            if (appUsers.Count() > 0)
                            {
                                foreach (var appUsr in appUsers)
                                {
                                    userapps.users.Remove(appUsr);

                                    await userapps.SaveChangesAsync();
                                }
                            }
                        }

                        var userapp = userapps.appusers.Where(ua => ua.appid == userApp.idapps).FirstOrDefault();

                        if (userapp != null)
                        {
                            userapps.appusers.Remove(userapp);
                            await userapps.SaveChangesAsync();
                        }
                    }

                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                           String.Format("Database error. Exception:{1}", ex.Message));
                    }
                }
            }
            return Request.CreateResponse<string>(HttpStatusCode.OK,
                                                  "User Appplication was deleted successfully.");
        }
        /// <summary>
        /// Creates the user application.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="systemuserid">The systemuserid.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> CreateUserApp(dynamic data)
        {
            int systemuserid = data.systemuserid;
            string appName = data.appName;

            app newApp = null;
            using (var uapps = new userappsEntities())
            {
                using (var sysuser = new exgripEntities())
                {
                    if (sysuser.UserProfiles.Any(u => u.UserId == systemuserid))
                    {
                        if (!uapps.apps.Any(a => a.appname.ToLower().Equals(appName.ToLower())))
                        {

                            try
                            {
                                uapps.ChangeTracker.DetectChanges();

                                newApp = new app()
                                {
                                    appname = appName, systemuserid = systemuserid
                                };

                                uapps.apps.Add(newApp);

                                await uapps.SaveChangesAsync();
                            }

                            catch (Exception ex)
                            {

                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.ToString());
                            }
                        }
                    }

                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Systemuser does not exist");
                    }
                }

            }
            return Request.CreateResponse<app>(newApp);
        }

        /// <summary>
        /// Deletes the user application user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> DeleteUserAppUser(dynamic data)
        {
            int userId = data.userId;
            int appId = data.appId;

            using (var userapps = new userappsEntities())
            {
                try
                {
                    userapps.ChangeTracker.DetectChanges();
                    var appUser = userapps.appusers.Where(usra => usra.appid == appId && usra.appid == appId).FirstOrDefault();

                    if (appUser != null)
                    {
                        userapps.appusers.Remove(appUser);
                        await userapps.SaveChangesAsync();
                    }


                }

                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                       String.Format("Database error. Exception:{1}", ex.Message));
                }
            }
            return Request.CreateResponse<string>(HttpStatusCode.OK, "App user deleted successfully.");
        }

        /// <summary>
        /// Adds the user application user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="systemuserid">The systemuserid.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> AddUserAppUser(dynamic data)
        {
            using (var userapps = new userappsEntities())
            {
                //Generate authentication data
                UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();

                int systemuserid = data.systemuserid;
                int appid = data.appId;

                var user = await authManger.IssueToken(systemuserid, appid);

                if (user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }

                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                       String.Format("Database error. Could not create application user."));
                }
            }
        }


        /// <summary>
        /// Lists all user apps.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public HttpResponseMessage ListAllUserApps(dynamic data)
        {

            string userName = data.userName;

            if (String.IsNullOrEmpty(userName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username cannot be null. Please supply a username.");
            }

            using (var userApps = new userappsEntities())
            {
                using (var system = new exgripEntities())
                {
                    if (!String.IsNullOrEmpty(userName))
                    {
                        var currentUser = system.UserProfiles.Where(usr => usr.UserName.ToLower().Equals(
                                              userName.ToLower())).FirstOrDefault();

                        if (currentUser == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not exist.");
                        }

                        else
                        {
                            var appsByUser = userApps.apps.Where(app => app.systemuserid == currentUser.UserId);

                            if (appsByUser == null)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User has no apps.");
                            }

                            else
                            {
                                if (appsByUser.Count() >= 1)
                                {
                                    return Request.CreateResponse<List<app>>(appsByUser.ToList());
                                }

                                else
                                {
                                    var usrApp = appsByUser.FirstOrDefault();

                                    if (usrApp == null)
                                    {
                                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User has no apps so far.");
                                    }

                                    return Request.CreateResponse<app>(usrApp);
                                }
                            }
                        }
                    }

                }
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No userapps available");
        }



        /// <summary>
        /// Gets the user application.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> SingleUserApp(dynamic data)
        {
            string appName = data.appName;

            if (String.IsNullOrEmpty(appName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Please supply a valid user application name.s");
            }

            return await Task<HttpResponseMessage>.Run(() =>
            {

                using (var userApps = new userappsEntities())
                {
                    var sysApp = userApps.apps.Where(a => a.appname.ToLower().Equals(appName.ToLower())).FirstOrDefault();

                    if (sysApp != null)
                    {
                        return Request.CreateResponse<app>(sysApp);
                    }

                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No such user app available.");
                    }
                }
            });
        }

        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> AddExternalUser(dynamic data)
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

            //Generate authentication data
            UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();
            using (var userApps = new userappsEntities())
            {
                userApps.ChangeTracker.DetectChanges();

                try
                {
                    var userExists = userApps.users.Any(uau => uau.username.ToLower().Equals(userName.ToLower()) && uau.appid == appId);

                    if (userExists)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User already exists.");
                    }

                    var pwd = await authManger.GeneratePasswordSalt(userName, password);

                    var user = new user()
                    {
                        username = userName, password = pwd, appid = appId
                    };

                    userApps.users.Add(user);

                    await userApps.SaveChangesAsync();

                    return Request.CreateResponse<user>(user);

                }

                catch (Exception ex)
                {

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
                }


            }


        }

        [HttpPost]
        [ApiAuthFilter]
        [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> RemoveExternalUser(dynamic data)
        {

            int userId = data.userId;
            string extUserName = data.extUserName;
            int appId = data.appId;

            #region checkParameters

            if (userId <= 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "External User id cannot be 0 or negative.");
            }

            if (appId <= 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User app id cannot be 0 or negative.");
            }

            if (string.IsNullOrEmpty(extUserName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "External Username cannot be null or empty.");
            }

            #endregion

            //Generate authentication data
            UserAppAuthenticationManager authManger = new Security.UserAppAuthenticationManager();
            using (var userApps = new userappsEntities())
            {
                userApps.ChangeTracker.DetectChanges();

                try
                {
                    var extUser = userApps.users.Where(uau => uau.iduser ==
                                                       userId && uau.username.ToLower().Equals(extUserName.ToLower()) && uau.appid == appId).FirstOrDefault();

                    if (extUser != null)
                    {
                        userApps.users.Remove(extUser);

                        await userApps.SaveChangesAsync();

                        return Request.CreateResponse<user>(extUser);
                    }

                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "External App user does not exist");
                    }

                }

                catch (Exception ex)
                {

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
                }


            }


        }

        #endregion



    }
}
