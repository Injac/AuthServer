using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Model
{
    public class AuthorizedIPRepository
    {
        public static IQueryable<string> GetAuthorizedIPs()
        {

            var ips = new List<string>();

            ips.Add("[WEBSERVER-IP]");

            //For debugging purposes only
            //ips.Add("127.0.0.1");
            //ips.Add("::1");

            return ips.AsQueryable();
        }
    }

}
