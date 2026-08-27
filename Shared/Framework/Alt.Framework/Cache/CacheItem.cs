using System;

namespace Alt.Framework.Cache
{
    internal class CacheItem
    {
        public DateTime? LatestCacheItemRetrieveDate { get; set; } = null;
        public int? RetrieveLifeTime { get; set; }
        public object Value { get; set; }
    }
}
