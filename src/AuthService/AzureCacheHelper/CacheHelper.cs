using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.AzureCacheHelper
{
    public class CacheHelper
    {

        private DataCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHelper"/> class.
        /// </summary>
        public CacheHelper(string namedCache)
        {
            if (string.IsNullOrEmpty(namedCache) || string.IsNullOrWhiteSpace(namedCache))
            {
                throw new ArgumentException("Parameter cannot be null, whitespace or empty.", "namedCache");
            }

            this.Init(namedCache);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init(string namedCache)
        {
           

            DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration(namedCache);
            config.IsCompressionEnabled = true;
            //config.MaxConnectionsToServer = 100000;
            config.RequestTimeout = TimeSpan.FromSeconds(10);
                      

            DataCacheFactory factory = new DataCacheFactory(config);

            this.cache = factory.GetCache(namedCache);

            this.cache.CreateRegion("ip");

        }


        /// <summary>
        /// Puts the object on cache.
        /// </summary>
        /// <param name="toPutOnCache">To put on cache.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public void PutObjectOnCache(object toPutOnCache, string identifier)
        {
          
                if (this.cache != null)
                {
                    cache.Put(identifier, toPutOnCache,"ip");
                }
            
        }

        /// <summary>
        /// Reads from cache.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public object ReadFromCache(string identifier)
        {
           
                if (this.cache != null)
                {
                    return cache.Get(identifier,"ip");
                }
                else
                {
                    return null;
                }
           
        }


        /// <summary>
        /// Removes from cache.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public bool RemoveFromCache(string identifier)
        {
            
           
                if (this.cache != null)
                {
                    return cache.Remove(identifier,"ip");
                }
                else
                {
                    return false;
                }
            
        }

        /// <summary>
        /// Clears the region.
        /// </summary>
        /// <param name="region">The region.</param>
        public void ClearRegion(string region)
        {
            this.cache.ClearRegion(region);
        }
    }
}
