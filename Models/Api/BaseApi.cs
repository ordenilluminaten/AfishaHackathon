using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Models.Extensions;

namespace Models.Api {
    public abstract class BaseApi {
        static BaseApi() {
            p_httpClient = new HttpClient();
        }
        public string ApiUrl { get; protected set; }
        //https://github.com/dotnet/corefx/issues/21568
        //Оказывается не очень круто диспозить клиента потому что падает производительноссть все говорят юзайте статик и диспозьте ответы
        protected static readonly HttpClient p_httpClient;
        public virtual async Task<string> GetAsync(string _url, object _data) {
            string resultContent;
            var urlQuery = _data?.ToGetUrlQuery();
            var resultingUrl = string.IsNullOrWhiteSpace(urlQuery) ? _url : $"{_url}?{urlQuery}";
            using (var response = await p_httpClient.GetAsync(resultingUrl)) {
                resultContent = await response.Content.ReadAsStringAsync();
            }
            return resultContent;
        }
        public virtual async Task<string> PostAsync(string _url, object _data) {
            string resultContent;
            var formData = _data?.ToFormData();
            var contentData = new FormUrlEncodedContent(formData);
            using (var response = await p_httpClient.PostAsync(_url, contentData)) {
                resultContent = await response.Content.ReadAsStringAsync();
            }
            return resultContent;

        }
        public virtual async Task<string> PostJsonAsync(string _url, object _data) {
            string resultContent;
            using (var response = await p_httpClient.PostAsync(_url, new StringContent(_data?.SerializeObject(),
                Encoding.UTF8, "application/json"))) {
                resultContent = await response.Content.ReadAsStringAsync();
            }
            return resultContent;
        }
    }
}