using AuthService.CsvMappings;
using AuthService.Filters;
using AuthService.Helpers;
using AuthService.Model;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.OData;

namespace AuthService.Controller
{
    [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
    public class PromotionCodesServiceController : ApiController
    {


        /// <summary>
        /// Gets all one time codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [IPHostValidationAttribute]
        public async Task<List<promotioncode>> GetAllOneTimeCodes(int top, int skip)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            else
            {
                return null;
            }


            var codes = await Task.Run<List<promotioncode>>(async () =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                        ent.Configuration.AutoDetectChangesEnabled = true;
                        ent.Configuration.LazyLoadingEnabled = false;
                        ent.ChangeTracker.DetectChanges();

                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false);

                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {
                                if (skip > promoCodesCurrentApiUser.Count())
                                {
                                    var retValue = promoCodesCurrentApiUser.ToList();

                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }

                                        ent.SaveChanges();
                                    }

                                    return retValue;
                                }
                                else
                                {


                                    var retValue = promoCodesCurrentApiUser.OrderBy(c => c.ID).Skip(skip).Take(top).ToList();


                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }

                                        
                                    }

                                   await ent.SaveChangesAsync();

                                    return retValue;


                                }

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
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;
        }

        /// <summary>
        /// Gets all one time codes sorted.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="column">The column.</param>
        /// <param name="ascDesc">The asc desc.</param>
        /// <returns></returns>
        [HttpGet]
        [IPHostValidationAttribute]
        public async Task<List<promotioncode>> GetAllOneTimeCodesSorted(int top, int skip, string column, string ascDesc)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            else
            {
                return null;
            }


            var codes = await Task.Run<List<promotioncode>>(async () =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                        ent.Configuration.AutoDetectChangesEnabled = true;
                        ent.Configuration.LazyLoadingEnabled = false;
                        ent.ChangeTracker.DetectChanges();

                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false);

                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {
                                if (skip > promoCodesCurrentApiUser.Count())
                                {
                                    var retValue = promoCodesCurrentApiUser.ToList();

                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }

                                        ent.SaveChanges();
                                    }

                                    return retValue;
                                }
                                else
                                {
                                    var retValue = new List<promotioncode>();

                                    switch (column)
                                    {
                                        case "promocode":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.promocode).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.promocode).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "timezone":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.timezone).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.timezone).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "link":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.GetCodeLink).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.GetCodeLink).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "validfrom":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.validfrom).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.validfrom).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "validTo":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.validuntil).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.validuntil).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "status":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.IsActive).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.IsActive).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "redeemed":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.redeemed).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.redeemed).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        default:
                                            break;
                                    }

                                    
                                  

                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                           // record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }


                                    }

                                    await ent.SaveChangesAsync();

                                    return retValue;


                                }

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
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;
        }

        /// <summary>
        /// Gets all multi codes sorted.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="column">The column.</param>
        /// <param name="ascDesc">The asc desc.</param>
        /// <returns></returns>
        [HttpGet]
        [IPHostValidationAttribute]
       public async Task<List<promotioncode>> GetAllMultiCodesSorted(int top, int skip, string column, string ascDesc)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            else
            {
                return null;
            }


            var codes = await Task.Run<List<promotioncode>>(async () =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                        ent.Configuration.AutoDetectChangesEnabled = true;
                        ent.Configuration.LazyLoadingEnabled = false;
                        ent.ChangeTracker.DetectChanges();

                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true);

                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {
                                if (skip > promoCodesCurrentApiUser.Count())
                                {
                                    var retValue = promoCodesCurrentApiUser.ToList();

                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }

                                        ent.SaveChanges();
                                    }

                                    return retValue;
                                }
                                else
                                {
                                    var retValue = new List<promotioncode>();

                                    switch (column)
                                    {
                                        case "promocode":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.promocode).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.promocode).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "timezone":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.timezone).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.timezone).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "link":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.GetCodeLink).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.GetCodeLink).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "validfrom":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.validfrom).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.validfrom).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "validTo":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.validuntil).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.validuntil).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "status":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.IsActive).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.IsActive).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        case "redeemed":
                                            if (ascDesc.Equals("desc"))
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderByDescending(c => c.redeemed).Skip(skip).Take(top).ToList();
                                            }
                                            else
                                            {
                                                retValue = promoCodesCurrentApiUser.OrderBy(c => c.redeemed).Skip(skip).Take(top).ToList();
                                            }
                                            break;
                                        default:
                                            break;
                                    }




                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }


                                    }

                                    await ent.SaveChangesAsync();

                                    return retValue;


                                }

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
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;
        }

        /// <summary>
        /// Gets the one time codes count.
        /// </summary>
        /// <returns></returns>
        [IPHostValidationAttribute]
        public async Task<int?> GetOneTimeCodesCount()
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;
            using (userappsEntities ent = new userappsEntities())
            {
                if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
                {
                    string appKey = Request.Headers.GetValues(APP_KEY).First();
                    string appSecret = Request.Headers.GetValues(APP_SECRET).First();

                    using (var sysapps = new userappsEntities())
                    {

                        user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                        if (user == null)
                        {
                            return null;
                        }
                        else
                        {
                            using (var exgrip = new exgripEntities())
                            {
                                profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                                if (profile == null)
                                {
                                    return null;
                                }

                            }
                        }

                    }

                }
                else
                {
                    return null;
                }


                var codesCount = await Task.Run<int?>(() =>
                {
                    try
                    {
                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false);

                        if (promoCodesCurrentApiUser != null)
                        {


                            return promoCodesCurrentApiUser.Count();


                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                });

                return codesCount;
            }
        }


        /// <summary>
        /// Gets the multi codes count count.
        /// </summary>
        /// <returns></returns>
        [IPHostValidationAttribute]
        public async Task<int?> GetMultiCodesCountCount()
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;
            using (userappsEntities ent = new userappsEntities())
            {
                if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
                {
                    string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                    string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                    using (var sysapps = new userappsEntities())
                    {

                        user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                        if (user == null)
                        {
                            return null;
                        }
                        else
                        {
                            using (var exgrip = new exgripEntities())
                            {
                                profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                                if (profile == null)
                                {
                                    return null;
                                }

                            }
                        }

                    }

                }
                else
                {
                    return null;
                }


                var codesCount = await Task.Run<int?>(() =>
                {
                    try
                    {
                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true);

                        if (promoCodesCurrentApiUser != null)
                        {


                            return promoCodesCurrentApiUser.Count();


                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                });

                return codesCount;
            }
        }

        /// <summary>
        /// Gets all multi codes.
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [IPHostValidationAttribute]
        public async Task<List<promotioncode>> GetAllMultiCodes(int top, int skip)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;
            using (userappsEntities ent = new userappsEntities())
            {

                ent.ChangeTracker.DetectChanges();

                if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
                {
                    string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                    string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                    using (var sysapps = new userappsEntities())
                    {

                        user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                        if (user == null)
                        {
                            return null;
                        }
                        else
                        {
                            using (var exgrip = new exgripEntities())
                            {
                                profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                                
                                if (profile == null)
                                {
                                    return null;
                                }

                            }
                        }

                    }

                }
                else
                {
                    return null;
                }


                return await Task.Run<List<promotioncode>>( async () =>
                {
                    try
                    {
                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true);


                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {
                                if (skip > promoCodesCurrentApiUser.Count())
                                {
                                    //Check, if the codes are still active
                                    foreach(var record in promoCodesCurrentApiUser)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);

                                        if (record.IsActive.Value == true)
                                        {
                                            if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                            {
                                                //record.IsActive = true;
                                            }
                                            else
                                            {
                                                record.IsActive = false;
                                            }
                                        }

                                       
                                    }
                                    await ent.SaveChangesAsync();
                                    return promoCodesCurrentApiUser.ToList();
                                }
                                else
                                {
                                    //Check, if the codes are still active

                                    var skipped =  promoCodesCurrentApiUser.OrderBy(c => c.ID).Skip(skip).Take(top);

                                    //Check, if the codes are still active
                                    foreach (var record in skipped)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);

                                        if (record.IsActive.Value == true)
                                        {
                                            if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                            {
                                                //record.IsActive = true;
                                            }
                                            else
                                            {
                                                record.IsActive = false;
                                            }

                                        }
                                    }
                                    await ent.SaveChangesAsync();
                                    return skipped.ToList();
                                }

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
                    catch (Exception ex)
                    {

                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                });
            }
        }


        /// <summary>
        /// Searches for promotion code.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="searchColumn">The search column.</param>
        /// <param name="multiOrNot">The multi or not.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<List<promotioncode>> SearchForPromotionCode(dynamic data)
        {

            //int top, int skip,string searchTerm,string searchColumn,string multiOrNot
            var searchColumn = (string)data.searchColumn;
            var skip = (int)data.skip;
            var top = (int)data.top;
            var searchTerm = (string)data.searchTerm;
            var multiOrNot = (bool)data.multiOrNot;

            #region Auth
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            else
            {
                return null;
            }

            #endregion
            var codes = await Task.Run<List<promotioncode>>(async () =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                        ent.Configuration.AutoDetectChangesEnabled = true;
                        ent.Configuration.LazyLoadingEnabled = false;
                        ent.ChangeTracker.DetectChanges();

                        var promoCodesCurrentApiUser = new List<promotioncode>();


                        switch (searchColumn)
                        {
                            case "promotioncode":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false &&
                                        code.promocode.Contains(searchTerm)).ToList();
                                }
                                else
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true &&
                                        code.promocode.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "timezone":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.timezone.Contains(searchTerm)).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                      && code.timezone.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "getcodelink":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.GetCodeLink.Contains(searchTerm)).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                       && code.GetCodeLink.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "status":

                                bool statOrNot = false;

                                if(searchTerm == "active")
                                {
                                    statOrNot = true;
                                }


                                if (multiOrNot)
                                {
                                    
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.IsActive == statOrNot).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                         && code.IsActive == statOrNot).ToList();
                                }
                                break;
                            case "redeemed":

                                bool redeemed = false;

                                if (searchTerm == "yes")
                                {
                                    redeemed = true;
                                }


                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.redeemed == redeemed).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                         && code.redeemed == redeemed).ToList();
                                }
                                break;
                            case "timespan":

                                DateTime start;
                                DateTime end;

                                var dateValues = searchTerm.Split('#');

                                var startOk = DateTime.TryParse(dateValues[0], out start);
                                var endOk = DateTime.TryParse(dateValues[1], out end);
                                
                               
                                if (startOk && endOk)
                                {
                                    if (multiOrNot)
                                    {

                                        promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                            
                                            && (code.validfrom >= start) && (code.validuntil <= end)).ToList();

                                    }
                                    else
                                    {
                                        promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true

                                          && (code.validfrom >= start) && (code.validuntil <= end)).ToList();
                                    }
                                }
                                else
                                {
                                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                                }
                                break;
                            default:
                                break;
                        }

                        


                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {
                                if (skip > promoCodesCurrentApiUser.Count())
                                {
                                    var retValue = promoCodesCurrentApiUser.ToList();

                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }

                                        ent.SaveChanges();
                                    }

                                    return retValue;
                                }
                                else
                                {


                                    var retValue = promoCodesCurrentApiUser.OrderBy(c => c.ID).Skip(skip).Take(top).ToList();


                                    //Check, if the codes are still active
                                    foreach (var record in retValue)
                                    {

                                        var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(record.timezone);

                                        var validFrom = TimeZoneInfo.ConvertTime(new DateTime(record.validfrom.Value.Year, record.validfrom.Value.Month, record.validfrom.Value.Day,
                                                                                            record.validfrom.Value.Hour, record.validfrom.Value.Minute, record.validfrom.Value.Second),
                                                                                 customerTimeZone,
                                                                                customerTimeZone);

                                        var validTo = TimeZoneInfo.ConvertTime(new DateTime(record.validuntil.Value.Year, record.validuntil.Value.Month, record.validuntil.Value.Day, record.validuntil.Value.Hour,
                                                                                    record.validuntil.Value.Minute, record.validuntil.Value.Second),
                                                                                   customerTimeZone,
                                                                                  customerTimeZone);


                                        if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                                        {
                                            //record.IsActive = true;
                                        }
                                        else
                                        {
                                            record.IsActive = false;
                                        }


                                    }

                                    await ent.SaveChangesAsync();

                                    return retValue;


                                }

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
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;

        }

        /// <summary>
        /// Searches for promotion code count.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<int> SearchForPromotionCodeCount(dynamic data)
        {

            //int top, int skip,string searchTerm,string searchColumn,string multiOrNot
            var searchColumn = (string)data.searchColumn;
            var skip = (int)data.skip;
            var top = (int)data.top;
            var searchTerm = (string)data.searchTerm;
            var multiOrNot = (bool)data.multiOrNot;

            #region Auth
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return 0;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return 0;
                            }

                        }
                    }

                }

            }
            else
            {
                return 0;
            }

            #endregion
            var codes = await Task.Run<int>(() =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                        ent.Configuration.AutoDetectChangesEnabled = true;
                        ent.Configuration.LazyLoadingEnabled = false;
                        ent.ChangeTracker.DetectChanges();

                        var promoCodesCurrentApiUser = new List<promotioncode>();


                        switch (searchColumn)
                        {
                            case "promotioncode":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false &&
                                        code.promocode.Contains(searchTerm)).ToList();
                                }
                                else
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true &&
                                        code.promocode.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "timezone":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.timezone.Contains(searchTerm)).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                      && code.timezone.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "getcodelink":
                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.GetCodeLink.Contains(searchTerm)).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                       && code.GetCodeLink.Contains(searchTerm)).ToList();
                                }
                                break;
                            case "status":

                                bool statOrNot = false;

                                if (searchTerm == "active")
                                {
                                    statOrNot = true;
                                }


                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.IsActive == statOrNot).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                         && code.IsActive == statOrNot).ToList();
                                }
                                break;
                            case "redeemed":

                                bool redeemed = false;

                                if (searchTerm == "yes")
                                {
                                    redeemed = true;
                                }


                                if (multiOrNot)
                                {

                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false
                                        && code.redeemed == redeemed).ToList();
                                }
                                else
                                {
                                    promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true
                                         && code.redeemed == redeemed).ToList();
                                }
                                break;
                            case "timespan":

                                DateTime start;
                                DateTime end;

                                var dateValues = searchTerm.Split('#');

                                var startOk = DateTime.TryParse(dateValues[0], out start);
                                var endOk = DateTime.TryParse(dateValues[1], out end);


                                if (startOk && endOk)
                                {
                                    if (multiOrNot)
                                    {

                                        promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == false

                                            && (code.validfrom >= start) && (code.validuntil <= end)).ToList();

                                    }
                                    else
                                    {
                                        promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId) && code.ismulticode == true

                                          && (code.validfrom >= start) && (code.validuntil <= end)).ToList();
                                    }
                                }
                                else
                                {
                                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                                }
                                break;
                            default:
                                break;
                        }




                        if (promoCodesCurrentApiUser != null)
                        {
                            return promoCodesCurrentApiUser.Count();
                        }

                        return 0;
                    }
                }
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;

        }

        /// <summary>
        /// Gets the promocodes CSV file.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [IPHostValidationAttribute]
        public async Task<HttpResponseMessage> GetPromocodesCsvFile()
        {
            #region Auth
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;

            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Uri.UnescapeDataString(Request.Headers.GetValues(APP_KEY).First());
                string appSecret = Uri.UnescapeDataString(Request.Headers.GetValues(APP_SECRET).First());

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return null;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            else
            {
                return null;
            } 
            #endregion

            MemoryStream csvFile = new MemoryStream();

            var codes = await Task.Run<HttpResponseMessage>( async () =>
            {
                try
                {
                    using (userappsEntities ent = new userappsEntities())
                    {
                     
                        var promoCodesCurrentApiUser = ent.promotioncodes.Where(code => code.userid.Equals(profile.AlternateUserId)).ToList();

                        //var PromoCodes = new List<PromoCode>();


                        //foreach(var pcode in promoCodesCurrentApiUser)
                        //{
                        //    var promoCode = new PromoCode();
                        //    promoCode.promocode = pcode.promocode;
                        //    promoCode.IsActive = pcode.IsActive.HasValue ? pcode.IsActive.Value : false;
                        //    promoCode.validfrom = pcode.validfrom.HasValue ? pcode.validfrom.Value : DateTime.Now;
                        //    promoCode.validuntil = pcode.validuntil.HasValue ? pcode.validuntil.Value : DateTime.Now;
                        //    promoCode.redeemed = pcode.redeemed.HasValue ? pcode.redeemed.Value : false;
                        //    promoCode.timezone = pcode.timezone;
                        //    promoCode.multicodequantity = pcode.multicodequantity.HasValue ? pcode.multicodequantity.Value : 0;
                        //    promoCode.multiredeemcount = pcode.multiredeemcount.HasValue ? pcode.multiredeemcount.Value : 0;
                        //    promoCode.ismulticode = pcode.ismulticode.HasValue ? pcode.ismulticode.Value : false;

                        //    PromoCodes.Add(promoCode);
                        //}

                        if (promoCodesCurrentApiUser != null)
                        {
                            if (promoCodesCurrentApiUser.Count() > 0)
                            {

                                         TextWriter csvWriter = new StreamWriter(csvFile);

                                         var firstLine = "Promotional Code,Redeemed,Valid From,Valid To,Is Multi User Code,Muti User Count,Multi User Count Redeemed,Time Zone,Get Code Url";
                                        await csvWriter.WriteLineAsync(firstLine);

                                        foreach (var prepared in promoCodesCurrentApiUser)
                                        {
                                            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                prepared.promocode,
                                                prepared.redeemed.Value ? "YES" : "NO",
                                                prepared.validfrom.Value.ToShortDateString(),
                                                prepared.validuntil.Value.ToShortDateString(),
                                                prepared.ismulticode.Value ? "YES" : "NO",
                                                prepared.multicodequantity.HasValue ? prepared.multicodequantity : 0,
                                                prepared.multiredeemcount.HasValue ? prepared.multiredeemcount : 0,
                                                prepared.timezone,
                                                prepared.GetCodeLink);

                                            await csvWriter.WriteLineAsync(line);
                                        }

                                        HttpResponseMessage response = new HttpResponseMessage();

                                        await csvWriter.FlushAsync();
                                      
                                        csvFile.Position = 0;

                                        response.Content = new StreamContent(csvFile);
                                                                               
                                        return response;

                                    
                                
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
                catch (Exception ex)
                {

                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            });

            return codes;
        }

        /// <summary>
        /// Gets the promotion codes count.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [IPHostValidationAttribute]
        public async Task<string> GetPromotionCodesCount(string id)
        {
            return await  Task.Run<string>(() =>
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                using (var sysapps = new userappsEntities())
                {

                    var promocodeCount = sysapps.promotioncodes.Where(code => code.userid.Equals(id)).Count();

                    if (promocodeCount != null)
                    {
                        return promocodeCount.ToString();

                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }

                }

               
            });
        }
    }
}
