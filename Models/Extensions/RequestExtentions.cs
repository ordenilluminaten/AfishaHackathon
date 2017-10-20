using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Models.Extensions {
    public static class RequestExtentions {
        public static async Task<string> ReadBodyAsync(this HttpRequest _httpRequest) {
            return await ReadBodyAsync(_httpRequest, null);
        }

        public static async Task<string> ReadBodyAsync(this HttpRequest _httpRequest, Encoding _encoding) {
            using (var reader = new StreamReader(_httpRequest.Body, _encoding ?? Encoding.UTF8)) {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<JObject> ReadBodyAsJObjectAsync(this HttpRequest _httpRequest) {
            return JObject.Parse(await ReadBodyAsync(_httpRequest, null)) ;
        }

        public static Task<TType> ReadBodyAsync<TType>(this HttpRequest _httpRequest) {
            return ReadBodyAsync<TType>(_httpRequest, null);
        }

        public static async Task<TType> ReadBodyAsync<TType>(this HttpRequest _httpRequest, Encoding _encoding) {
            using (var reader = new StreamReader(_httpRequest.Body, _encoding ?? Encoding.UTF8)) {
                return JsonConvert.DeserializeObject<TType>(await reader.ReadToEndAsync());
            }
        }
    }
}
