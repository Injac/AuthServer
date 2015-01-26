using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Storage
{
    /// <summary>
    /// User Quota for blob-storage container.
    /// </summary>
    public interface IDirectoryQuota
    {
       
        /// <summary>
        /// Checks the user quota.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Task<long> CheckUserQuota();
    }
}
