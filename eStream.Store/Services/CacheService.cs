using System;
using System.Collections.Concurrent;
using System.Web;
using Cache = System.Web.Caching.Cache;

namespace Estream.Cart42.Web.Services
{
    public class CacheService : ICacheService
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> _invalidationKeys
            = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        public T Get<T>(string cacheId, Func<T> getItemCallback, string invalidateKeys = null,
            DateTime? absoluteExpiration = null, TimeSpan? slidingExpiration = null) where T : class
        {
            object cacheItem = HttpRuntime.Cache.Get(cacheId);
            if (cacheItem is NullValue) return null;
            var item = cacheItem as T;
            if (item == null && getItemCallback != null)
            {
                item = getItemCallback();
                cacheItem = item;
                if (item == null) cacheItem = new NullValue();
                HttpContext.Current.Cache.Insert(cacheId, cacheItem, null,
                    absoluteExpiration ?? Cache.NoAbsoluteExpiration, 
                    slidingExpiration ?? Cache.NoSlidingExpiration);
            }

            if (!string.IsNullOrEmpty(invalidateKeys))
            {
                foreach (var key in invalidateKeys.Split(','))
                {
                    var bag = _invalidationKeys.GetOrAdd(key, new ConcurrentBag<string>());
                    bag.Add(cacheId);
                }
            }

            return item;
        }

        public void Remove(string cacheId)
        {
            HttpRuntime.Cache.Remove(cacheId);
        }

        public void Invalidate(string invalidateKeys)
        {
            if (!string.IsNullOrEmpty(invalidateKeys))
            {
                foreach (var key in invalidateKeys.Split(','))
                {
                    ConcurrentBag<string> bag;
                    if (_invalidationKeys.TryGetValue(key, out bag))
                    {
                        string cacheId;
                        while (bag.TryTake(out cacheId))
                        {
                            Remove(cacheId);
                        }
                    }
                }
            }
        }
    }

    public struct NullValue { }
}