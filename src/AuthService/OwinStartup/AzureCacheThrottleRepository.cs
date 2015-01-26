using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiThrottle;
using AuthService.AzureCacheHelper;

namespace AuthService.OwinStartup
{
    /// <summary>
    /// The Cache throttle repository.
    /// This cache never expires.
    /// Let's see how fast it will work.
    /// </summary>
    public class AzureCacheThrottleRepository : IThrottleRepository
    {
        private CacheHelper _chelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCacheThrottleRepository"/> class.
        /// </summary>
        public AzureCacheThrottleRepository()
        {
            _chelper = new CacheHelper("appadditives");
        }
        /// <summary>
        /// Anies the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool Any(string id)
        {
            return _chelper.ReadFromCache(id) != null;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _chelper.ClearRegion("ip");
        }

        /// <summary>
        /// Firsts the or default.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ThrottleCounter? FirstOrDefault(string id)
        {
            return (ThrottleCounter?)_chelper.ReadFromCache(id);
        }

        /// <summary>
        /// Removes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Remove(string id)
        {
            _chelper.RemoveFromCache(id);
        }

        /// <summary>
        /// Saves the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="throttleCounter">The throttle counter.</param>
        /// <param name="expirationTime">The expiration time.</param>
        public void Save(string id, ThrottleCounter throttleCounter, TimeSpan expirationTime)
        {
            if(_chelper.ReadFromCache(id) != null)
            {
                _chelper.PutObjectOnCache(throttleCounter, id);
            }
        }
    }
}
