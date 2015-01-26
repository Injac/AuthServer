using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Model;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using System.Web.Security;

namespace AuthService.Security
{
    /// <summary>
    /// Authentication for user added apps.
    /// Each user can add additional apps
    /// based on subscriptions.
    /// </summary>
    public class UserAppAuthenticationManager
    {
    
        /// <summary>
        /// Issues the token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="systemuserid">The systemuserid.</param>
        /// <param name="appId">The application identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// or
        /// Userid cannot be zero or negative.;systemuserid
        /// or
        /// Appid cannot be zero or negative.;appId
        /// </exception>
        public async Task<User> IssueToken ( int systemuserid, int appId )
        {
            #region CheckParameters
            //if ( String.IsNullOrEmpty ( username ) )
            //{
            //    throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            //}
            
            //if ( String.IsNullOrEmpty ( password ) )
            //{
            //    throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            //}
            if ( systemuserid <= 0 )
            {
                throw new ArgumentException ( "Userid cannot be zero or negative.", "systemuserid" );
            }
            
            if ( appId <= 0 )
            {
                throw new ArgumentException ( "Appid cannot be zero or negative.", "appId" );
            }
            
            #endregion
            TaskCompletionSource<User> tks = new TaskCompletionSource<User> ();

            using ( var userapps = new userappsEntities () )
            {
                using ( var sysuser = new exgripEntities () )
                {
                    userapps.ChangeTracker.DetectChanges ();
                    //Check if app exeists
                    var currentApp = userapps.apps.Where ( a => a.idapps == appId &&
                                                           a.systemuserid == systemuserid ).FirstOrDefault ();
                    var currentUser = sysuser.UserProfiles.Where ( usr => usr.UserId ==
                                      systemuserid ).FirstOrDefault ();

                   

                    var password = Membership.GeneratePassword ( 15, 5 );
                    
                    if ( currentUser == null )
                    {
                        tks.SetResult ( null );
                        return tks.Task.Result;
                    }
                    
                    if ( currentApp != null )
                    {
                        var encrptedPassword = await GeneratePasswordSalt ( currentUser.UserName, password );
                        var user = await EncryptToken ( currentUser.UserName, encrptedPassword, password, true );

                        userapps.appusers.Add ( new appuser ()
                        {
                            appSecret = user.Secret,
                            apptoken = user.Token,
                            appid = currentApp.idapps,
                            securitySoup = user.SecSoup
                        } );
                        
                        try
                        {
                            await userapps.SaveChangesAsync ();
                        }
                        
                        catch ( Exception ex )
                        {
                        }
                        
                        tks.SetResult ( user );
                    }
                    
                    else
                    {
                        try
                        {
                            var encrptedPassword = await GeneratePasswordSalt ( currentUser.UserName, password );
                            var user = await EncryptToken ( currentUser.UserName, encrptedPassword, password );
                            //Update existing user
                            var existingUser = userapps.appusers.Where ( ua => ua.appid ==
                                               currentApp.idapps ).FirstOrDefault ();
                            existingUser.appSecret = user.Secret;
                            existingUser.apptoken = user.Token;
                            existingUser.securitySoup = user.SecSoup;
                            await userapps.SaveChangesAsync ();
                            tks.SetResult ( user );
                        }
                        
                        catch ( Exception ex )
                        {
                            throw;
                        }
                    }
                }
            }
            return tks.Task.Result;
        }
        
        public async Task<User> IssueTokenSysApp ( string username, string password, int systemuserid,
                int appId )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( username ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( password ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            if ( systemuserid <= 0 )
            {
                throw new ArgumentException ( "Userid cannot be zero or negative.", "systemuserid" );
            }
            
            if ( appId <= 0 )
            {
                throw new ArgumentException ( "Appid cannot be zero or negative.", "appId" );
            }
            
            #endregion
            TaskCompletionSource<User> tks = new TaskCompletionSource<User> ();
            var encrptedPassword = await GeneratePasswordSalt ( username, password );
            var user = await EncryptToken ( username, encrptedPassword, password, true );
            
            if ( user != null )
            {
                tks.SetResult ( user );
                return tks.Task.Result;
            }
            
            else
            {
                tks.SetResult ( null );
                return tks.Task.Result;
            }
        }
        
        /// <summary>
        /// Validates the system application token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="secret">The secret.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// </exception>
        public async Task<bool> ValidateSystemAppToken ( string token, string secret )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( token ) )
            {
                throw new ArgumentException ( "Token cannot be null or empty.", "token" );
            }
            
            if ( String.IsNullOrEmpty ( secret ) )
            {
                throw new ArgumentException ( "Secret cannot be null or empty", "secret" );
            }
            
            #endregion
            var tokenValue = await DecryptTokenSystemApp ( token, secret );
            TaskCompletionSource<bool> tks = new TaskCompletionSource<bool> ();
            
            if ( !String.IsNullOrEmpty ( tokenValue ) )
            {
                var values = tokenValue.Split ( new string[] {";#;"}, StringSplitOptions.RemoveEmptyEntries );
                
                if ( values == null )
                {
                    tks.SetResult ( false );
                }
                
                else
                {
                    if ( values.Count () > 1 )
                    {
                        using ( var sysUsers = new userappsEntities () )
                        {
                            var userName = sysUsers.systemappusers.Where ( sau => sau.apptoken.Equals ( token )
                                           && sau.appSecret.Equals ( secret ) ).FirstOrDefault ();
                                           
                            if ( ! ( userName == null ) )
                            {
                                tks.SetResult ( true );
                            }
                            
                            else
                            {
                                tks.SetResult ( false );
                            }
                        }
                    }
                }
            }
            
            else
            {
                tks.SetResult ( false );
            }
            
            return tks.Task.Result;
        }
        /// <summary>
        /// Validates the user application token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="passwordOrig">The password original.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// </exception>
        public async Task<bool> ValidateUserAppToken ( string token, string secret, string passwordOrig )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( token ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( secret ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            var tokenValue = await DecryptToken ( token, secret );
            TaskCompletionSource<bool> tks = new TaskCompletionSource<bool> ();
            
            if ( !String.IsNullOrEmpty ( tokenValue ) )
            {
                var values = tokenValue.Split ( new string[] {";#;"}, StringSplitOptions.RemoveEmptyEntries );
                
                if ( values == null )
                {
                    tks.SetResult ( false );
                }
                
                else
                {
                    if ( values.LongLength > 1 )
                    {
                        var result =  await ValidateAppUser ( values[0], values[1], passwordOrig );
                        tks.SetResult ( result );
                    }
                }
            }
            
            else
            {
                tks.SetResult ( false );
            }
            
            return tks.Task.Result;
        }
        /// <summary>
        /// Decrypts the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="userid">The userid.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// or
        /// Userid cannot be negative.;userid
        /// </exception>
        private Task<string> DecryptToken ( string token, string secret )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( token ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( secret ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            appuser encryptedSoup;
            using ( var userapps = new userappsEntities () )
            {
                encryptedSoup = userapps.appusers.Where ( uapp => uapp.appSecret.Equals (
                                    secret )
                                && uapp.apptoken.Equals ( token )
                                                        ).FirstOrDefault ();
            }
            TaskCompletionSource<string> tks = new TaskCompletionSource<string> ();
            
            if ( encryptedSoup == null )
            {
                tks.SetResult ( string.Empty );
                return tks.Task;
            }
            
            var privTest = new
            System.Security.Cryptography.X509Certificates.X509Certificate2 (
                @"Certificates\private_key.pfx", "01fjsjgSzn6V2iMMpDPO" );
            RSACryptoServiceProvider cryptoProvidor2 = ( RSACryptoServiceProvider )
                    privTest.PrivateKey;
            byte[] decryptedTokenBytes = cryptoProvidor2.Decrypt (
                                             Convert.FromBase64String ( encryptedSoup.securitySoup ), true );
            var decrypt = Encoding.UTF8.GetString ( decryptedTokenBytes );
            
            if ( decrypt != null )
            {
                tks.SetResult ( decrypt );
                return tks.Task;
            }
            
            else
            {
                tks.SetResult ( string.Empty );
                return tks.Task;
            }
        }
        private Task<string> DecryptTokenSystemApp ( string token, string secret )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( token ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( secret ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            systemappuser encryptedSoup;
            using ( var userapps = new userappsEntities () )
            {
                encryptedSoup = userapps.systemappusers.Where ( uapp => uapp.appSecret.Equals (
                                    secret )
                                && uapp.apptoken.Equals ( token )
                                                              ).FirstOrDefault ();
            }
            TaskCompletionSource<string> tks = new TaskCompletionSource<string> ();
            
            if ( encryptedSoup == null )
            {
                tks.SetResult ( string.Empty );
                return tks.Task;
            }
            
            var privTest = new
            System.Security.Cryptography.X509Certificates.X509Certificate2 (
                @"Certificates\private_key.pfx", "01fjsjgSzn6V2iMMpDPO" );
            RSACryptoServiceProvider cryptoProvidor2 = ( RSACryptoServiceProvider )
                    privTest.PrivateKey;
            byte[] decryptedTokenBytes = cryptoProvidor2.Decrypt (
                                             Convert.FromBase64String ( encryptedSoup.securitySoup ), true );
            var decrypt = Encoding.UTF8.GetString ( decryptedTokenBytes );
            
            if ( decrypt != null )
            {
                tks.SetResult ( decrypt );
                return tks.Task;
            }
            
            else
            {
                tks.SetResult ( string.Empty );
                return tks.Task;
            }
        }
        /// <summary>
        /// Encrypts the token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// or
        /// Userdata is not valid. Please check your credentials
        /// </exception>
        private  Task<User> EncryptToken ( string username, string password, string passswordOrig,
                                           bool isNew = false )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( username ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( password ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            #region UserValidation
            //if ( !isNew )
            //{
            //    var userValid = await this.ValidateAppUser ( username, password, passswordOrig );
            //    if ( !userValid )
            //    {
            //        throw new ArgumentException ( "Userdata is not valid. Please check your credentials" );
            //    }
            //}
            #endregion
            #region EncryptValues
            var test = new System.Security.Cryptography.X509Certificates.X509Certificate2 (
                @"Certificates\DER.cer" );
            var encrypted = test.Issuer;
            RSACryptoServiceProvider cryptoProvidor = ( RSACryptoServiceProvider )
                    test.PublicKey.Key;
            var toEncrypt = string.Format ( "{0};#;{1}", username, password );
            byte[] encryptedTokenBytes = cryptoProvidor.Encrypt ( Encoding.UTF8.GetBytes (
                                             toEncrypt ), true );
            var encrptedValue = Convert.ToBase64String ( encryptedTokenBytes );
            #endregion
            var user = new User
            {
                UserName = username,
                Token = encrptedValue.Substring ( 0, 64 ),
                Secret = encrptedValue.Substring ( 66, 64 ),
                SecSoup = encrptedValue
            };
            var tks = new TaskCompletionSource<User> ();
            tks.SetResult ( user );
            return tks.Task;
        }
        /// <summary>
        /// Validates the application user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Username cannot be null or empty.;username
        /// or
        /// Password cannot be null or empty.;password
        /// </exception>
        private Task<bool> ValidateAppUser ( string username, string password, string passwordOrig )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( username ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( password ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            TaskCompletionSource<bool> tks = new TaskCompletionSource<bool> ();
            using ( var sysUser = new exgripEntities () )
            {
                var user = sysUser.UserProfiles.Where ( u => u.UserName.ToUpper ().Equals(
                                                      username.ToUpper () )
                                                  ).FirstOrDefault ();
                                                  
                if ( user == null )
                {
                    tks.SetResult ( false );
                }
                else
                {
                    using (var userapps = new userappsEntities())
                    {
                        var appUserExists = userapps.apps.Any(ua => ua.systemuserid == user.UserId);

                        if(appUserExists)
                        {
                            tks.SetResult(true);
                        }
                        else
                        {
                            tks.SetResult(false);
                        }
                    }
                }
               
               
            }
            return tks.Task;
        }
        private Task<bool> ValidateAppUser ( int appId, int systemUserId )
        {
            #region CheckParameters
            if ( appId <= 0 )
            {
                throw new ArgumentException ( "Application id cannot be zero or negative", "appId" );
            }
            
            if ( systemUserId <= 0 )
            {
                throw new ArgumentException ( "User id cannot be zero or negative.", "systemUserId" );
            }
            
            #endregion
            TaskCompletionSource<bool> tks = new TaskCompletionSource<bool> ();
            using ( var userapps = new userappsEntities () )
            {
                userapps.ChangeTracker.DetectChanges ();
                using ( var sysUsers = new exgripEntities () )
                {
                    sysUsers.ChangeTracker.DetectChanges ();
                    var sysUser = sysUsers.UserProfiles.Where ( usr => usr.UserId == systemUserId ).FirstOrDefault ();
                    
                    if ( sysUser == null )
                    {
                        tks.SetResult ( false );
                    }
                    
                    else
                    {
                        var sysApp = userapps.systemapps.Where ( sa => sa.id == appId ).FirstOrDefault ();
                        
                        if ( sysApp == null )
                        {
                            tks.SetResult ( false );
                        }
                        
                        else
                        {
                            var sysAppUsr = userapps.systemappusers.Any ( sau => sau.appid == sysApp.id &&
                                            sau.systemuserid == sysUser.UserId );
                            tks.SetResult ( sysAppUsr );
                        }
                    }
                }
            }
            return tks.Task;
        }
        /// <summary>
        /// Generates the password salt.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns></returns>
        public Task<string> GeneratePasswordSalt ( string user, string userPassword )
        {
            #region CheckParameters
            if ( String.IsNullOrEmpty ( user ) )
            {
                throw new ArgumentException ( "Username cannot be null or empty.", "username" );
            }
            
            if ( String.IsNullOrEmpty ( userPassword ) )
            {
                throw new ArgumentException ( "Password cannot be null or empty.", "password" );
            }
            
            #endregion
            string pwdToHash = userPassword + "$2a$&sdjkj#s";
            string hashToStoreInDatabase = BCrypt.HashPassword ( pwdToHash, BCrypt.GenerateSalt () );
            TaskCompletionSource<string> tks = new TaskCompletionSource<string> ();
            
            if ( !String.IsNullOrEmpty ( hashToStoreInDatabase ) )
            {
                tks.SetResult ( hashToStoreInDatabase );
            }
            
            return tks.Task;
        }
        /// <summary>
        /// Doeses the password match.
        /// </summary>
        /// <param name="hashedPwdFromDatabase">The hashed password from database.</param>
        /// <param name="userEnteredPassword">The user entered password.</param>
        /// <returns></returns>
        public bool DoesPasswordMatch ( string hashedPwdFromDatabase, string userEnteredPassword )
        {
            return BCrypt.CheckPassword ( userEnteredPassword + "$2a$&sdjkj#s", hashedPwdFromDatabase );
        }

       
    }
    
    
    
}
