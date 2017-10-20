using Newtonsoft.Json;

namespace Models.Extensions {
    public static class ObjectExtensions {
        public static string SerializeObject(this object _object) {
            return JsonConvert.SerializeObject(_object);
        }

    }
}
