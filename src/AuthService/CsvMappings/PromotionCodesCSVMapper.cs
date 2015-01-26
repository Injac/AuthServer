using AuthService.Model;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.CsvMappings
{
    /// <summary>
    /// Maps the output for the csv file.
    /// Not all fields need to be exported.
    /// </summary>
    public class PromotionCodesCSVMapper:CsvClassMap<PromoCode>
    {

        public override void CreateMap()
        {
            //thisDate.ToString("d")

            Map(m => m.promocode).Name("Promotional Code");
            Map(m => m.IsActive).Name("Code Active").TypeConverterOption(true, "ACTIVE").TypeConverterOption(false, "DEACTIVATED");
            Map(m => m.redeemed).Name("Code Redemeed").TypeConverterOption(true, "YES").TypeConverterOption(false, "NO");
            Map(m => m.ismulticode).Name("Multi User Code").TypeConverterOption(true, "YES").TypeConverterOption(false, "NO");
            Map(m => m.multicodequantity).Name("Multi User Count");
            Map(m => m.multiredeemcount).Name("Multi User Code Redeem Count");
            Map(m => m.validfrom).Name("Code Valid From");
            Map(m => m.validuntil).Name("Code Valid Until");
            Map(m => m.timezone).Name("Timezone");

        }


         
    }

    public  class PromoCode
    {
        public int ID { get; set; }
        public string userid { get; set; }
        public string promocode { get; set; }
        public bool redeemed { get; set; }
        public bool ismulticode { get; set; }
        public int multicodequantity { get; set; }
        public DateTime validuntil { get; set; }
        public string timezone { get; set; }
        public bool codevalid { get; set; }
        public string GetCodeLink { get; set; }
        public bool IsActive { get; set; }
        public DateTime created { get; set; }
        public DateTime validfrom { get; set; }
        public int multiredeemcount { get; set; }
    }
}
