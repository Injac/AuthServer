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
using Core.Crypto;

namespace AuthService.Controller
{
    /// <summary>
    /// The OTP Server.
    /// </summary>
    public class OtpController: ApiController
    {
    
        private const string LogName = "OTSLOG";

        private const int TimeDiffAllowed = 8;

        /// <summary>
        /// Generates the new secret.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> GenerateNewSecret ( int userId, int appId )
        {
            Logging.Logger logger = new Logging.Logger(LogName);

            try
            {
                using (var userApps = new userappsEntities())
                {

                    userApps.ChangeTracker.DetectChanges();
                    var otpUser = userApps.OTPUsers.Where(otpu => otpu.appid == appId &&
                                                            otpu.userid == userId).FirstOrDefault();

                    if (otpUser != null)
                    {
                        byte[] secret;
                        OTP.Helper.RandomHelper.GenerateRandomByteArray(20, out secret);
                        var serialized = await JsonConvert.SerializeObjectAsync(secret);
                        otpUser.secret = serialized;
                        await userApps.SaveChangesAsync();

                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("SECRETSUCCESS  {0} for appId {1} generated."
                                    , userId, appId), LogName));

                        return Request.CreateResponse<string>(serialized);
                    }

                    else
                    {

                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("SECRETGENERROR  {0} for appId {1}"
                                   , userId, appId), LogName));

                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                             "Application user has no OTP access");
                    }


                }
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                   ,"GenereteNewSecret",ex.ToString()), LogName)).Wait();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                                            "Database Error");
            }
        }

        /// <summary>
        /// Adds the new otp user.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> AddNewOtpUser(dynamic authData)
        {

            if (ReferenceEquals(null, authData.userId) || Equals(0, authData.userId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero.");
            }

            if (ReferenceEquals(null, authData.appId) || Equals(0, authData.appId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero.");
            }

            Logging.Logger logger = new Logging.Logger(LogName);

            int userId = authData.userId;
            int appId = authData.appId;
            byte[] secret;
            OTP.Helper.RandomHelper.GenerateRandomByteArray(20, out secret);
           
            var serialized = await JsonConvert.SerializeObjectAsync(secret);

            try
            {
                using (var userApps = new userappsEntities())
                {

                    userApps.ChangeTracker.DetectChanges();

                    if (userApps.appusers.Any(usr => usr.appid == appId && usr.idappusers == userId))
                    {
                        if (!userApps.OTPUsers.Any(otp => otp.userid == userId && otp.appid == appId))
                        {
                            var otpUser = userApps.OTPUsers.Add(new OTPUser()
                            {
                                userid = userId,
                                appid = appId,
                                secret =
                                    serialized,
                                seqvalid = true,
                                otpcounter = 1,
                                otpcreated = DateTime.UtcNow
                            });

                            await userApps.SaveChangesAsync();
                            var value = new { Message = "User successfully added." };
                            var ser = await JsonConvert.SerializeObjectAsync(value);
                            await logger.StoreNewLogMessage(new Logging.Message(String.Format("ADDUSERSUCCESS  {0} for appId {1} generated."
                                  , userId, appId), LogName));
                            return Request.CreateResponse<string>(ser);
                        }
                        else
                        {
                            var value = new { Message = "User already exists" };
                            var ser = await JsonConvert.SerializeObjectAsync(value);
                            logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                 , "AddNewOtpUser", "User already exists."), LogName)).Wait();
                            return Request.CreateResponse<string>(ser);
                        }
                    }
                    else
                    {
                        var value = new { Message = "User is not an appuser." };
                        logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "AddNewOtpUser", "User is not an app user"), LogName)).Wait();
                        var ser = await JsonConvert.SerializeObjectAsync(value);
                        return Request.CreateResponse<string>(ser);
                    }
                }
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                  , "AddNewOtpUser", ex.ToString()), LogName)).Wait();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                                            "Database Error");
            }

            
        }


        /// <summary>
        /// Removes the otp user.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> RemoveOtpUser(dynamic authData)
        {
            if (ReferenceEquals(null, authData.userId) || Equals(0, authData.userId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero.");
            }

            if (ReferenceEquals(null, authData.appId) || Equals(0, authData.appId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero.");
            }

            Logging.Logger logger = new Logging.Logger(LogName);

            try
            {
                int userId = authData.userId;
                int appId = authData.appId;

                using (var userApps = new userappsEntities())
                {
                    userApps.ChangeTracker.DetectChanges();

                    var user = userApps.OTPUsers.Where(otpu => otpu.appid == appId &&
                        otpu.userid == userId).FirstOrDefault();

                    if (user != null)
                    {
                        userApps.OTPUsers.Remove(user);
                        await userApps.SaveChangesAsync();
                        var value = new { Message = "Otp user successfully removed", UserId = userId, AppId = appId };
                        var ser = await JsonConvert.SerializeObjectAsync(value);
                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("REMOVETOPUSERSUCCESS  {0} for appId {1} generated."
                                , userId, appId), LogName));
                        return Request.CreateResponse<string>(ser);
                    }
                    else
                    {
                        var value = new { Message = "Otp user does not exist.", UserId = userId, AppId = appId };
                        var ser = await JsonConvert.SerializeObjectAsync(value);
                        logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "RemoveOtpUser", "OTP User does not exist."), LogName)).Wait();
                        return Request.CreateResponse<string>(ser);
                    }


                }
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                  , "RemoveOtpUser", ex.ToString()), LogName)).Wait();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                                            "Database Error");
            }
        }
        
        /// <summary>
        /// Gets the current otp.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> GetCurrentOtpServer ( dynamic authData )
        {
            if ( ReferenceEquals ( String.Empty, authData.userId ) || Equals ( 0, authData.userId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero." );
            }
            
            if ( ReferenceEquals ( string.Empty, authData.appId ) || Equals ( 0, authData.appId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero." );
            }

            Logging.Logger logger = new Logging.Logger(LogName);

            try
            {
                int userId = 0;
                int appId = 0;

               

                try
                {


                    userId = authData.userId;
                    appId = authData.appId;

                    if (appId <= 0 || userId <= 0)
                    {
                        logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "GetCurrentOtpServer", "Parameters not valid."), LogName)).Wait();
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Parameters are not valid");
                    }
                }
                catch (Exception ex)
                {
                    logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "GetCurrentOtpServer", "Parameters not valid."), LogName)).Wait();
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Parameters are not valid");

                }


                var otpValid = await IsCurrentOtpValid(userId, appId);

                if (otpValid.QuerySuccess == false && otpValid.SeqValid == false)
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("GETCURRENTTOPSERVERERROR  Userid {0} for appId {1} generated. Error {2}"
                                 , userId, appId, "User is not valid."), LogName));
                }
                else
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("GETCURRENTTOPSERVERSUCCESS  Userid {0} for appId {1} generated."
                                     , userId, appId), LogName));
                }

                return Request.CreateResponse<OtpCheckData>(otpValid);
            }
            catch (Exception ex)
            {
                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "GetCurrentTopServer", ex.ToString()), LogName)).Wait();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                                            "Database Error");
                
            }
        }

        /// <summary>
        /// Determines whether [is current otp valid] [the specified user id].
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="appId">The app id.</param>
        /// <returns></returns>
        private async Task<OtpCheckData> IsCurrentOtpValid(int userId, int appId)
        {
          
                Logging.Logger logger = new Logging.Logger(LogName);

                try
                {
                    using (var userApps = new userappsEntities())
                    {
                        var counter = userApps.OTPUsers.Where(otp => otp.appid == appId &&
                                                                otp.userid == userId).FirstOrDefault();

                        if (counter == null)
                        {
                            return new OtpCheckData() { QuerySuccess = false, SeqValid = false };
                        }

                        TimeSpan result = DateTime.UtcNow - counter.otpcreated;
                        var secret = await GetCurrentUserOtpSecret(userId, appId);

                        var otpAlgo = new Core.Crypto.OTPAlgo((ulong)counter.otpcounter, secret);

                        var currentOtp = otpAlgo.GetCurrentOTP();

                        if (counter != null)
                        {

                            if (result.TotalMinutes > TimeDiffAllowed)
                            {
                                await logger.StoreNewLogMessage(new Logging.Message(
                                    String.Format("OTPCHECKSUCCES by user {0} for appId {1} for counter {2}"
                                    , userId, appId, "counter invalid"), LogName));

                                return new OtpCheckData()
                                {
                                    QuerySuccess = true,
                                    SeqValid = false,
                                    CurrentOtp = currentOtp,
                                    PassedTime = result.TotalMinutes
                                };
                            }
                            else
                            {
                                await logger.StoreNewLogMessage(new Logging.Message(String.Format("OTPCHECKSUCCES by user {0} for appId {1} for counter {2}"
                                   , userId, appId, "valid"), LogName));


                                return new OtpCheckData()
                                {
                                    QuerySuccess = true,
                                    SeqValid = true,
                                    CurrentOtp = currentOtp,
                                    PassedTime = result.TotalMinutes
                                };
                            }

                        }

                        else
                        {
                            await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "GetCurrentTopServer", "Counter not found"), LogName));
                            return new OtpCheckData() { QuerySuccess = false, SeqValid = false };
                        }
                    }
                }
                catch (Exception ex)
                {

                    logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "IsCurrentOtpValid", ex.ToString()), LogName)).Wait();
                    return new OtpCheckData()
                    {
                        QuerySuccess = false,
                        SeqValid = false,
                        CurrentOtp = string.Empty,
                        PassedTime = 0
                    };
                   
                }
           
        }

        private class OtpCheckData
        {
            /// <summary>
            /// Gets or sets the seq valid.
            /// </summary>
            /// <value>The seq valid.</value>
            public bool SeqValid { get; set; }

            /// <summary>
            /// Gets or sets the query success.
            /// </summary>
            /// <value>The query success.</value>
            public bool QuerySuccess { get; set; }

            /// <summary>
            /// Gets or sets the otp counter.
            /// </summary>
            /// <value>The otp counter.</value>
            //public long OtpCounter { get; set; }

            /// <summary>
            /// Gets or sets the current otp.
            /// </summary>
            /// <value>The current otp.</value>
            public string CurrentOtp { get; set; }

            /// <summary>
            /// Gets or sets the passed time.
            /// </summary>
            /// <value>The passed time.</value>
            public double PassedTime { get; set; }
        }
       
        /// <summary>
        /// Otps the login.
        /// </summary>
        /// <param name="authData">The auth data.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> OtpLogin(dynamic authData)
        {

            #region Check Input Data
            if (ReferenceEquals(null, authData.userId) || Equals(0, authData.userId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero.");
            }

            if (ReferenceEquals(null, authData.appId) || Equals(0, authData.appId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero.");
            }

            if (ReferenceEquals(null, authData.Otp) || Equals(String.Empty, authData.Otp))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero.");
            }
            #endregion
            Logging.Logger logger = new Logging.Logger(LogName);
            try
            {
               

                int userId = authData.userId;
                int appId = authData.appId;
                string otp = authData.Otp;


                if(userId <= 0)
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "OtpLogin", "Userid invalid."), LogName));
                    return Request.CreateResponse<string>("PARAMERRORUSERID");
                }

                if(appId <= 0)
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "OtpLogin", "AppId invalid."), LogName));
                    return Request.CreateResponse<string>("PARAMERRORAPPID");
                }

                if(String.IsNullOrEmpty(otp))
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "OtpLogin", "Otp invalid."), LogName));
                    return Request.CreateResponse<string>("PARAMERROROTP");
                }


                var otpValid = await IsCurrentOtpValid(userId, appId);

                if(!otpValid.SeqValid)
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                               , "OtpLogin", "OTP Invalid"), LogName));
                    return Request.CreateResponse<string>("OTPINVALID");
                }

                var counter = await GetDbOtpCounterValue(userId, appId);
                var secret = await GetCurrentUserOtpSecret(userId, appId);

                OTPAlgo algo = new OTPAlgo((ulong)counter, secret);

                var currentOtp = algo.GetCurrentOTP();

                if (currentOtp.Equals(otp))
                {
                    //Successfull, create a new secret, and create a new otp
                    var invalid = await SetOtpCounterInvalid(userId, appId);

                    if (invalid)
                    {
                        await GenerateNewSecret(userId, appId);
                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("OTPLOGINSUCCES by user {0} for appId {1}", userId, appId),
                            LogName));
                        return Request.CreateResponse<string>("SUCCESS");
                    }
                    else
                    {
                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                               , "OtpLogin", "Counter could not be set valid."), LogName));
                        return Request.CreateResponse<string>("INVALIDATEERROR");
                    }
                }
                else
                {
                    await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "OtpLogin", "Wrong OTP."), LogName));
                    //Not successfull, check if OTP is still valid
                    return Request.CreateResponse<string>("WRONGOTP");
                }
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                 , "OtpLogin", ex.ToString()), LogName)).Wait();

                return Request.CreateResponse<string>("SYSERROR");
            }

        }
        
        /// <summary>
        /// Gets the next otp.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> GetNextOtpServer ( dynamic authData )
        {
            #region Check Input Data
            if ( ReferenceEquals ( null, authData.userId ) || Equals ( 0, authData.userId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero." );
            }
            
            if ( ReferenceEquals ( null, authData.appId ) || Equals ( 0, authData.appId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero." );
            }
            
            //if ( ReferenceEquals ( null, authData.localCounter ) || Equals ( 0, authData.localCounter ) )
            //{
            //    return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
            //                                         "The localCounter value cannot be null or zero." );
            //}
            
            #endregion

            Logging.Logger logger = new Logging.Logger(LogName);

            int userId = authData.userId;
            int appId  = authData.appId;
            
            //First sync that Data, use it later
            //var syncData = await SyncOtpCounterHere ( authData );

            //var syncDataObject = await JsonConvert.DeserializeObjectAsync ( syncData );
            
            var secret = await GetCurrentUserOtpSecret ( userId,appId  );

            long srvCounter = await GetDbOtpCounterValue(userId, appId);

            if(srvCounter == -1)
            {
                var errorData = new
                {
                    LastOtp = "",
                    NewOtp ="",
                    NewCounter = ""
                };

                await logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                               , "GetNextOtpServer", "Parameter Errror."), LogName));


                var error = await JsonConvert.SerializeObjectAsync(errorData);

                return Request.CreateResponse<string>(error);
            }

            try
            {
                OTPAlgo algo = new OTPAlgo((ulong)srvCounter, secret);

                var currentOtp = algo.GetCurrentOTP();

                //invalidate the current otp
                await SetOtpCounterInvalid(userId, appId);

                //Get the next otp, save and set it valid
                var nextOtp = algo.GetNextOTP();


                //This updates the OTP counter and sets it valid.
                await SetOtpCounterValid(userId, appId, (long)algo.Counter);

                var nextOtpData = new
                {
                    LastOtp = currentOtp,
                    NewOtp = nextOtp,
                };

                var serialized = await JsonConvert.SerializeObjectAsync(nextOtpData);

                return Request.CreateResponse<string>(serialized);
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                 , "GetNextOtpServer", ex.ToString()), LogName)).Wait();

                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        /// <summary>
        /// Sets the otp counter invalid.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        private async Task<bool> SetOtpCounterInvalid(int userId, int appId)
        {
            Logging.Logger logger = new Logging.Logger(LogName);

            try
            {
                using (var userApps = new userappsEntities())
                {
                    userApps.ChangeTracker.DetectChanges();

                    var otpData = userApps.OTPUsers.Where(usrOtp => usrOtp.userid == userId && usrOtp.appid == appId).FirstOrDefault();

                    if (otpData != null)
                    {
                        otpData.seqvalid = false;
                        otpData.otpcreated = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(20));
                        await userApps.SaveChangesAsync();

                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("SETOTPCOUNTERINVALIDSUCCESS by user {0} for appId {1}"
                                  , userId, appId), LogName));

                        return true;
                    }
                    else
                    {
                        await logger.StoreNewLogMessage(new Logging.Message(String.Format("SETOTPCOUNTERINVALIDERROR by user {0} for appId {1}"
                                 , userId, appId), LogName));
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {

                logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                                , "SetOtpCounterInvalid", ex.ToString()), LogName)).Wait();

                return false;
            }
        }


        /// <summary>
        /// Sets the otp counter valid.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        private async Task<bool> SetOtpCounterValid(int userId, int appId, long counter)
        {
            Logging.Logger logger = new Logging.Logger(LogName);

            using (var userApps = new userappsEntities())
            {
                userApps.ChangeTracker.DetectChanges();

                var otpData = userApps.OTPUsers.Where(usrOtp => usrOtp.userid == userId && usrOtp.appid == appId).FirstOrDefault();

                if (otpData != null)
                {
                    otpData.seqvalid = true;
                    otpData.otpcounter = counter;
                    otpData.otpcreated = DateTime.UtcNow;
                    await userApps.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        
        /// <summary>
        /// Synchronizes the otp counter.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthFilter]
        public async Task<HttpResponseMessage> SyncOtpCounter ( dynamic authData )
        {
            Logging.Logger logger = new Logging.Logger(LogName);

            if ( ReferenceEquals ( null, authData.localCounter ) || Equals ( 0, authData.localCounter ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The localCounter value cannot be null or zero." );
            }
            
            if ( ReferenceEquals ( null, authData.userId ) || Equals ( 0, authData.userId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The userId value cannot be null or zero." );
            }
            
            if ( ReferenceEquals ( null, authData.appId ) || Equals ( 0, authData.appId ) )
            {
                return Request.CreateErrorResponse ( HttpStatusCode.BadRequest,
                                                     "The appId value cannot be null or zero." );
            }
            
            long localCounter = authData.localCounter;
            int userId = authData.userId;
            int appId = authData.appId;
            var srvCounterValue = await GetDbOtpCounterValue ( userId, appId );
            
            if ( localCounter > srvCounterValue )
            {
                //update server counter
                var updateSuccess = await UpdateServerCounter ( localCounter, userId, appId );
                var updateScenario = new { UpdateServer = true, LocalCounter = localCounter,
                                           ServerCounter = localCounter, UpdateSuccess = updateSuccess, isEqual = false
                                         };
                var serialized = JsonConvert.SerializeObject ( updateScenario );
                return Request.CreateResponse<string> ( serialized );
            }
            
            else
                if ( localCounter < srvCounterValue )
                {
                    //update local counter
                    var updateScenario = new { UpdateServer = false, LocalCounter = srvCounterValue, ServerCounter = srvCounterValue, isEqual = false };
                    var serialized = JsonConvert.SerializeObject ( updateScenario );
                    return Request.CreateResponse<string> ( serialized );
                }
                
                else
                    if ( localCounter == srvCounterValue )
                    {
                        var updateScenario = new { UpdateServer = false, LocalCounter = srvCounterValue, ServerCounter = srvCounterValue, isEqual = true };
                        var serialized = JsonConvert.SerializeObject ( updateScenario );
                        return Request.CreateResponse<string> ( serialized );
                    }
                    
                    else
                    {
                        return Request.CreateResponse<string> ( "SyncError" );
                    }
        }
        
        /// <summary>
        /// Synchronizes the otp counter.
        /// </summary>
        /// <param name="authData">The authentication data.</param>
        /// <returns></returns>
        
        private async Task<string> SyncOtpCounterHere ( dynamic authData )
        {

            Logging.Logger logger = new Logging.Logger(LogName);

            long localCounter = authData.localCounter;
            int userId = authData.userId;
            int appId = authData.appId;
            
            var srvCounterValue = await GetDbOtpCounterValue ( userId, appId );
           
            string serialized = string.Empty;

            if ( localCounter > srvCounterValue )
            {
                //update server counter
                var updateSuccess = await UpdateServerCounter ( localCounter, userId, appId );
                var updateScenario = new { UpdateServer = true, LocalCounter = localCounter, 
                    ServerCounter = localCounter, UpdateSuccess = updateSuccess, IsEqual=false };
                serialized = JsonConvert.SerializeObject ( updateScenario );
               
            }

            else if (localCounter < srvCounterValue)
            {
                //update local counter
                var updateScenario = new { UpdateServer = false, LocalCounter = srvCounterValue,
                    ServerCounter = srvCounterValue, IsEqual = false };
                 serialized = JsonConvert.SerializeObject ( updateScenario );
                
            }
            else if (localCounter == srvCounterValue)
            {
                  var updateScenario = new { UpdateServer = false, LocalCounter = localCounter,
                    ServerCounter = srvCounterValue, IsEqual = true };
                serialized = JsonConvert.SerializeObject ( updateScenario );
               
            }

            return serialized;
        }
        
        /// <summary>
        /// Updates the server counter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        private async Task<bool> UpdateServerCounter ( long value, int userId, int appId )
        {
            Logging.Logger logger = new Logging.Logger ( LogName );
            
            try
            {
                using ( var userApps = new userappsEntities () )
                {
                    userApps.ChangeTracker.DetectChanges();
                    var counter = userApps.OTPUsers.Where ( otp => otp.appid == appId &&
                                                            otp.userid == userId ).FirstOrDefault ();
                                                            
                    if ( counter != null )
                    {
                        counter.otpcounter = value-1;
                        counter.otpcreated = DateTime.UtcNow;

                        await userApps.SaveChangesAsync ();
                        await logger.StoreNewLogMessage ( new Logging.Message (
                                                              String.Format ( "SUCESS. COUNTER UPDATE ON SERVER. User:{0}, App:{1}", userId, appId ),
                                                              LogName ) );
                        return true;
                    }
                    
                    else
                    {
                        return false;
                    }
                }
            }
            
            catch ( Exception ex )
            {
                logger.StoreNewLogMessage ( new Logging.Message (
                                                String.Format ( "Error during UpdateCounter. User:{0}, App:{1}, Error:{2}",
                                                        userId, appId, ex.ToString () ), LogName ) ).Wait ();
                return false;
            }
        }
        
        /// <summary>
        /// Gets the database otp counter value.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        private async Task<long> GetDbOtpCounterValue ( int userId, int appId )
        {
            return await Task.Run<long> ( () =>
            {
                Logging.Logger logger = new Logging.Logger(LogName);

                try
                {
                    using (var userApps = new userappsEntities())
                    {
                        var counter = userApps.OTPUsers.Where(otp => otp.appid == appId &&
                                                                otp.userid == userId).FirstOrDefault();

                        if (counter != null)
                        {

                            logger.StoreNewLogMessage(new Logging.Message(String.Format("GETDBCOUNTERVALUESUCCESS by user {0} for appId {1}"
                                 , userId, appId), LogName)).Wait();

                            return counter.otpcounter;
                        }

                        else
                        {
                            logger.StoreNewLogMessage(new Logging.Message(String.Format("GETDBCOUNTERVALUEERROR by user {0} for appId {1} Error:{2}"
                                , userId, appId, "user not found"), LogName)).Wait();
                            return -1;
                        }
                    }
                }
                catch (Exception ex)
                {

                    logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                               , "GetDBOtpCounter", ex.ToString()), LogName)).Wait();

                    return -1;
                }
            } );
        }
        
        /// <summary>
        /// Gets the current user otp secret.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        private async Task<byte[]> GetCurrentUserOtpSecret ( int userId, int appId )
        {
            return await Task.Run<byte[]> ( () =>
            {
                using ( var userApps = new userappsEntities () )
                {
                    Logging.Logger logger = new Logging.Logger(LogName);

                    try
                    {
                        var counter = userApps.OTPUsers.Where(otp => otp.appid == appId &&
                                                                            otp.userid == userId).FirstOrDefault();

                        if (counter != null)
                        {
                            var secretBytes = JsonConvert.DeserializeObject<byte[]>(counter.secret);
                            logger.StoreNewLogMessage(new Logging.Message(String.Format("GETCURRENTUSERTOPSECRETSUCCESS by user {0} for appId {1}"
                                 , userId, appId), LogName)).Wait();
                            return secretBytes;
                        }

                        else
                        {
                            logger.StoreNewLogMessage(new Logging.Message(String.Format("GETCURRENTUSERTOPSECRETERROR by user {0} for appId {1} Error {2}"
                                , userId, appId,"User not found"), LogName)).Wait();
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.StoreNewLogMessage(new Logging.Message(String.Format("APPERROR, METHOD {0} ERROR {1}"
                              , "GetCurrentUserOtpSecret", ex.ToString()), LogName)).Wait();
                        return null;
                    }
                }
            } );
        }
    }
}
