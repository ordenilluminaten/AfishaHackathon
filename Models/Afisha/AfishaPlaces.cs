using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Models.Afisha {
    public class AfishaData {
        private readonly IMemoryCache p_cache;
        public const string CachePlaceDictKey = "places";
        public const string CacheCityDictKey = "cities";

        public AfishaData(IMemoryCache _cache, FeedParser _parser) {
            p_cache = _cache;
            _parser.Parse().Wait();
        }
        public Dictionary<int, Place> Places => p_cache.Get<Dictionary<int, Place>>(CachePlaceDictKey);
        public Dictionary<int, string> Cities => p_cache.Get<Dictionary<int, string>>(CacheCityDictKey);
    }
}