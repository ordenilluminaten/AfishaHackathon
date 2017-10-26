using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Models.Extensions {
    public static class SessionExtentions {
        public static TType Get<TType>(this ISession _session, string _key) {
            var value = _session.GetString(_key);
            if (value == null)
                return default(TType);
            var obj = JsonConvert.DeserializeObject<TType>(value);
            return obj;
        }
        public static void Set<TType>(this ISession session, string key, TType value) {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
