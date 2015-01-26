using AuthService.Filters;
using AuthService.Helpers;
using AuthService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace AuthService.Controller
{
    [EnableCors(origins: "http://www.appadditives.com", headers: "*", methods: "*")]
    public class PromotionalCodesController:ApiController
    {
        /// <summary>
        /// Posts the redeem.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> PostRedeem(dynamic data)
        {

            if(data == null)
            {
                return false;
            }


            var value = (string)data.promocode;
            

        
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            
            using (userappsEntities ctx = new userappsEntities())
            {
                try
                {
                    ctx.ChangeTracker.DetectChanges();

                    var code = ctx.promotioncodes.Where(x => x.promocode.Equals(value)  && (x.IsActive==true)).FirstOrDefault();

                    if (code == null)
                    {
                        return false;
                    }

                 
                    var userTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(code.timezone);
                   
                    //If code is valid to a certain date
                    if (code.validuntil.HasValue)
                    {
                        if (((DateTime.UtcNow + userTimeZone.GetUtcOffset(code.validuntil.Value)).Ticks < code.validuntil.Value.Ticks) && code.redeemed.Value == false)
                        {
                            if (!code.ismulticode.Value)
                            {
                                if (!code.redeemed.Value)
                                {
                                    code.redeemed = true;
                                    code.IsActive = false;
                                    await ctx.SaveChangesAsync();
                                    return true;
                                }
                            }

                            if (code.ismulticode.Value)
                            {


                                if (code.multicodequantity.HasValue)
                                    {
                                        if (code.multicodequantity > 0)
                                        {

                                            if (code.multicodequantity == 0)
                                            {
                                                code.redeemed = true;
                                                code.IsActive = false;
                                                await ctx.SaveChangesAsync();
                                                return false;
                                            }

                                            if (code.multicodequantity > 1)
                                            {
                                                code.multicodequantity = code.multicodequantity - 1;
                                                if (code.multiredeemcount.HasValue)
                                                {
                                                    code.multiredeemcount = code.multiredeemcount + 1;
                                                }
                                                else
                                                {
                                                    code.multiredeemcount = 1;
                                                }
                                                await ctx.SaveChangesAsync();

                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                            
                                        }
                                        else
                                        {
                                            if (code.IsActive.Value)
                                            {
                                                code.IsActive = false;
                                                await ctx.SaveChangesAsync();
                                            }

                                            return false;
                                        }  
                                    }
                                    else
                                    {
                                        return false;
                                    }
                               
                            }

                        }
                    }


                   
                  
                }
                catch (Exception ex)
                {
                    return false;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the multi code redeem values.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> GetMultiCodeRedeemValues(dynamic data)
        {

            string returnValue = string.Empty;

         
               if (data == null)
               {

                   dynamic retData =
                       new
                       {
                           status = "Argument Error. Please supply a valid promocode.",
                           availableRedeems = 0,
                           redeemCount = 0
                       };

                   await Task.Factory.StartNew(() =>
                   {
                       returnValue = Newtonsoft.Json.JsonConvert.SerializeObject(retData);
                   });

                   if(retData != string.Empty)
                   {
                       return retData;
                   }
                   else
                   {
                       return string.Empty;
                   }
               }


               var value = (string)data.promocode;



               if (string.IsNullOrWhiteSpace(value))
               {
                   var retData =
                       new
                       {
                           status = "Argument Error. Please supply a valid promocode.",
                           availableRedeems = 0,
                           redeemCount = 0
                       };

                   await Task.Factory.StartNew(() =>
                   {
                       returnValue = Newtonsoft.Json.JsonConvert.SerializeObject(retData);
                   });

                   if (returnValue != string.Empty)
                   {
                       return returnValue;
                   }
                   else
                   {
                       return string.Empty;
                   }
               }

               using (userappsEntities ctx = new userappsEntities())
               {
                   var code = ctx.promotioncodes.Where(x => x.promocode.Equals(value) && x.IsActive == true && x.ismulticode == true).FirstOrDefault();

                   if (code != null)
                   {
                       dynamic retData =
                       new
                       {
                           status = "Success",
                           availableRedeems = code.multicodequantity,
                           redeemCount = code.multiredeemcount
                           
                       };

                       await Task.Factory.StartNew(() =>
                       {
                           returnValue = Newtonsoft.Json.JsonConvert.SerializeObject(retData);
                       });

                       if (returnValue != string.Empty)
                       {
                           return returnValue;
                       }
                       else
                       {
                           return string.Empty;
                       }
                   }
                   else
                   {
                       dynamic retData =
                       new
                       {
                           status = "PromoCode Error. The supplied promocode does not seem to be valid.",
                           availableRedeems = 0,
                           redeemCount = 0
                       };

                       await Task.Factory.StartNew(() =>
                       {
                           returnValue = Newtonsoft.Json.JsonConvert.SerializeObject(retData);
                       });

                       if (returnValue != string.Empty)
                       {
                           return returnValue;
                       }
                       else
                       {
                           return string.Empty;
                       }
                   }
               }
          
        }

        /// <summary>
        /// Posts the validate.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> PostValidate(dynamic data)
        {

            return await Task.Run<bool>( async () =>
            {
                using (userappsEntities ctx = new userappsEntities())
                {

                    if (data == null)
                    {
                        return false;
                    }

                    var value = (string)data.promocode;

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return false;
                    }

                  

                    ctx.ChangeTracker.DetectChanges();

                    try
                    {
                        var code = ctx.promotioncodes.Where(x => x.promocode.Equals(value) && x.IsActive == true).FirstOrDefault();

                        if (code == null)
                        {
                            return false;
                        }


                        var userTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(code.timezone);

                        //If code is valid to a certain date
                        if (code.validuntil.HasValue)
                        {

                            //if code is a multicode
                            if (code.ismulticode.HasValue)
                            {
                                if (code.ismulticode.Value == true)
                                {
                                    if (code.validuntil.HasValue)
                                    {
                                        if (((DateTime.UtcNow + userTimeZone.GetUtcOffset(code.validuntil.Value)).Ticks < code.validuntil.Value.Ticks))
                                        {
                                            if (code.multicodequantity.HasValue)
                                            {
                                                if (code.multicodequantity.Value > 0)
                                                {
                                                    if (code.multiredeemcount.HasValue)
                                                    {
                                                        if (code.multicodequantity.Value > code.multiredeemcount.Value)
                                                        {
                                                            return true;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (code.multicodequantity.Value > 0)
                                                        {
                                                            return true;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    code.IsActive = false;
                                                    code.redeemed = true;
                                                    await ctx.SaveChangesAsync();
                                                    return false;
                                                }
                                            }
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (((DateTime.UtcNow + userTimeZone.GetUtcOffset(code.validuntil.Value)).Ticks < code.validuntil.Value.Ticks) && code.redeemed.Value == false)
                                    {
                                        if (code.redeemed.Value == false && code.IsActive.Value == true)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            if(code.redeemed.Value == true)
                                            {
                                                if(code.IsActive.Value == true)
                                                {
                                                    code.IsActive = false;
                                                    await ctx.SaveChangesAsync();
                                                    return false;
                                                }

                                                return false;
                                            }
                                            
                                            
                                        }
                                    }
                                    else
                                    {
                                        //Code is not in the validation time range
                                        code.IsActive = false;
                                        code.redeemed = true;
                                        await ctx.SaveChangesAsync();
                                        return false;
                                    }
                                }

                            }
                                                                                    
                        }


                        return false;
                      
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Posts the create new stack.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<bool> PostCreateNewStack(dynamic data)
        {
            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;
            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Request.Headers.GetValues(APP_KEY).First();
                string appSecret = Request.Headers.GetValues(APP_SECRET).First();

                using(var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();
                    
                   
                    if(user == null)
                    {
                        return false;
                    }
                    else
                    {
                        using(var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up=>up.UserId == user.systemuserid).FirstOrDefault();

                            if(profile == null)
                            {
                                return false;
                            }

                        }
                    }

               }
                
            }
            else
            {
                return false;
            }

            
            int betaCount = 500;

            WordGenerator gen = new WordGenerator();

           
            

            var timeZone = (string)data.timeZone;
            var dateString = (string)data.dateString;
            var dateStringFrom = (string)data.dateStringFrom;
            var codeLink = (string)data.codeLink;
            var userId = profile.AlternateUserId;
            var count = (int)data.count;





            DateTime outDate;

            var parseResult = DateTime.TryParse(dateString, out outDate);

            if (!parseResult)
            {
                return false;
            }

            DateTime outDate2;

            var parseResult2 = DateTime.TryParse(dateStringFrom, out outDate2);

            if (!parseResult2)
            {
                return false;
            }

            if(count > betaCount)
            {
                return false;
            }

            using (userappsEntities ctx = new userappsEntities())
            {
                try
                {
                    var customerTime = TimeZoneInfo.ConvertTime(new DateTime(outDate.Year, outDate.Month, outDate.Day, outDate.Hour, outDate.Minute, outDate.Second),
                                                              DateHelpers.GetTimeZoneInfoForTzdbId(timeZone),
                                                              DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));

                    var customerTime2 = TimeZoneInfo.ConvertTime(new DateTime(outDate2.Year, outDate2.Month, outDate2.Day, outDate2.Hour, outDate2.Minute, outDate2.Second),
                                                               DateHelpers.GetTimeZoneInfoForTzdbId(timeZone),
                                                               DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));

                    if ((customerTime2.Ticks > customerTime.Ticks))
                    {
                        return false;
                    }

                    var reedemedVouchers = ctx.promotioncodes.Where(x => x.userid == userId && x.redeemed == true && x.ismulticode == false).ToList();

                    var allOnetimes = ctx.promotioncodes.Where(x => x.userid == userId && x.ismulticode == false).ToList();

                    if((count + allOnetimes.Count()) > betaCount)
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                    }

                    if ((allOnetimes.Count() == betaCount) && (reedemedVouchers.Count < betaCount) && (reedemedVouchers.Count != 0))
                    {
                        return false;
                    }
                    else
                    {
                        ctx.Configuration.AutoDetectChangesEnabled = false;
                        ctx.Configuration.ValidateOnSaveEnabled = false;

                        for (int i = 1; i <= count; i++)
                        {
                            var word = gen.RandomString(7);

                            promotioncode code = new promotioncode();

                            code.created = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));
                            code.validfrom = customerTime2;
                            code.validuntil = customerTime;
                            code.redeemed = false;
                            code.promocode = word;
                            code.userid = userId;
                            code.ismulticode = false;
                            code.timezone = timeZone;
                            code.GetCodeLink = codeLink;
                            code.IsActive = true;
                            ctx.promotioncodes.Add(code);
                        }

                        await ctx.SaveChangesAsync();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Posts the create new multi user stack.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<bool> PostCreateNewMultiUserCode(dynamic data)
        {

            const string APP_KEY = "X-AppKey";
            const string APP_SECRET = "X-Token";

            systemappuser user = null;
            UserProfile profile = null;
            if (Request.Headers.Contains(APP_KEY) && Request.Headers.Contains(APP_SECRET))
            {
                string appKey = Request.Headers.GetValues(APP_KEY).First();
                string appSecret = Request.Headers.GetValues(APP_SECRET).First();

                using (var sysapps = new userappsEntities())
                {

                    user = sysapps.systemappusers.Where(usr => usr.appSecret.Equals(appSecret) && usr.apptoken.Equals(appKey)).FirstOrDefault();


                    if (user == null)
                    {
                        return false;
                    }
                    else
                    {
                        using (var exgrip = new exgripEntities())
                        {
                            profile = exgrip.UserProfiles.Where(up => up.UserId == user.systemuserid).FirstOrDefault();

                            if (profile == null)
                            {
                                return false;
                            }

                        }
                    }

                }

            }
            else
            {
                return false;
            }

            WordGenerator gen = new WordGenerator();

            var userId = profile.AlternateUserId;
            var timeZone = (string)data.timeZone;
            var amountOfUsers = (int)data.count;
            var dateString = (string)data.dateString;
            var dateStringFrom = (string)data.dateStringFrom;
            var codeLink = (string)data.codeLink;
            var count = (int)data.count;

            int betacount = 200;

            if(string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if(string.IsNullOrEmpty(timeZone) || string.IsNullOrWhiteSpace(timeZone))
            {
                return false;
            }

            if(amountOfUsers <=0)
            {
                return false;
            }

            DateTime outDate;

            var parseResult = DateTime.TryParse(dateString,out outDate);

            if(!parseResult)
            {
                return false;
            }


            DateTime outDate2;

            var parseResult2 = DateTime.TryParse(dateStringFrom, out outDate2);

            if (!parseResult2)
            {
                return false;
            }

            if(DateHelpers.GetTimeZoneInfoForTzdbId(timeZone) == null)
            {
                return false;
            }
            
            if(!string.IsNullOrWhiteSpace(codeLink))
            {
                Uri uriResult;
                bool result = Uri.TryCreate(codeLink, UriKind.Absolute, out uriResult);

                if(!result)
                {
                    return false;
                }
            }

            if(amountOfUsers == 0 || amountOfUsers <= 0 || amountOfUsers > int.MaxValue)
            {
                return false;
            }

            using (userappsEntities ctx = new userappsEntities())
            {
                try
                {

                    var customerTime = TimeZoneInfo.ConvertTime(new DateTime(outDate.Year, outDate.Month, outDate.Day,outDate.Hour, outDate.Minute, outDate.Second),
                                                                DateHelpers.GetTimeZoneInfoForTzdbId(timeZone),
                                                                DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));

                    var customerTime2 = TimeZoneInfo.ConvertTime(new DateTime(outDate2.Year, outDate2.Month, outDate2.Day, outDate2.Hour, outDate2.Minute, outDate2.Second),
                                                               DateHelpers.GetTimeZoneInfoForTzdbId(timeZone),
                                                               DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));

                    var allMultiCodes = ctx.promotioncodes.Where(x => x.userid == userId && x.ismulticode == true).ToList();


                    if ((allMultiCodes.Count()) > betacount)
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                    }

                    if(count > 2000000)
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                    }

                    if((customerTime2.Ticks > customerTime.Ticks))
                    {
                        return false;
                    }
                    
                    var word = gen.RandomString(7);

                    promotioncode code = new promotioncode();
                    code.created = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DateHelpers.GetTimeZoneInfoForTzdbId(timeZone));
                                                             
                    code.redeemed = false;
                    code.promocode = word;
                    code.userid = userId;
                    code.timezone = timeZone;
                    code.multicodequantity = amountOfUsers;
                    code.validfrom = customerTime2;
                    code.validuntil = customerTime;
                    code.GetCodeLink = codeLink;
                    code.IsActive = true;
                    code.ismulticode = true;

                    ctx.promotioncodes.Add(code);
                      
                    await ctx.SaveChangesAsync();

                    return true;
                    
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Remove promo code
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="promoCode">The promo code.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<bool> PostRemovePromoCode(dynamic data)
        {
           if(data == null)
           {
               throw new HttpResponseException(HttpStatusCode.BadRequest);
           }
                     

           //string userId, string promoCode
            using (userappsEntities ctx = new userappsEntities())
            {

                try
                {
                    var userId = (string)data.userId;
                    var promoCode = (string)data.promoCode;

                    var code = ctx.promotioncodes.Where(x => x.promocode.Equals(promoCode) && x.userid == userId).FirstOrDefault();

                    if (code == null)
                    {
                        return false;
                    }

                    ctx.promotioncodes.Remove(code);

                   await ctx.SaveChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Posts the de activiate promo code.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="promoCode">The promo code.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<bool> PostDeActiviatePromoCode(dynamic data)
        {

            if (data == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            using (userappsEntities ctx = new userappsEntities())
            {
              
                

                try
                {
                    ctx.ChangeTracker.DetectChanges();

                    var userId = (string)data.userId;
                    var promoCode = (string)data.promoCode;

                    var code = ctx.promotioncodes.Where(x => x.promocode.Equals(promoCode) && x.userid == userId).FirstOrDefault();

                    if (code == null)
                    {
                        return false;
                    }

                    code.IsActive = false;

                    await ctx.SaveChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Posts the activate promo code.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="promoCode">The promo code.</param>
        /// <returns></returns>
        [HttpPost]
        [IPHostValidationAttribute]
        public async Task<bool> PostActivatePromoCode(dynamic data)
        {

            if (data == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            using (userappsEntities ctx = new userappsEntities())
            {

                ctx.ChangeTracker.DetectChanges();
                ctx.Configuration.AutoDetectChangesEnabled = true;
                ctx.Configuration.LazyLoadingEnabled = false;

                try
                {
                    var userId = (string)data.userId;
                    var promoCode = (string)data.promoCode;

                    var code = ctx.promotioncodes.Where(x => x.promocode.Equals(promoCode) && x.userid == userId).FirstOrDefault();

                    if (code == null)
                    {
                        return false;
                    }

                    var customerTimeZone = DateHelpers.GetTimeZoneInfoForTzdbId(code.timezone);

                    var validFrom = TimeZoneInfo.ConvertTime(new DateTime(code.validfrom.Value.Year, code.validfrom.Value.Month, code.validfrom.Value.Day,
                                                                        code.validfrom.Value.Hour, code.validfrom.Value.Minute, code.validfrom.Value.Second),
                                                             customerTimeZone,
                                                            customerTimeZone);

                    var validTo = TimeZoneInfo.ConvertTime(new DateTime(code.validuntil.Value.Year, code.validuntil.Value.Month, code.validuntil.Value.Day, code.validuntil.Value.Hour,
                                                                code.validuntil.Value.Minute, code.validuntil.Value.Second),
                                                               customerTimeZone,
                                                              customerTimeZone);

                    if ((validFrom <= validTo) && (validTo > (DateTime.UtcNow + customerTimeZone.GetUtcOffset(validTo))))
                    {
                        if(code.redeemed.HasValue)
                        {
                            if(code.redeemed.Value)
                            {
                                code.IsActive = false;
                                await ctx.SaveChangesAsync();

                                return true;
                            }
                        }
                        code.IsActive = true;
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        return false;
                    }
                   

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
