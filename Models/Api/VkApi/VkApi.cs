using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Models.AppSettings;

namespace Models.Api.VkApi {
    public class VkApi : BaseApi {

        public VkApi(IOptions<AppSetting> _appSettings) {
            VkApiSettings = _appSettings.Value.VkApiSettings;
            ApiUrl = VkApiSettings.ApiUrl;
            Messages = new Messages(this);

        }
        public Messages Messages { get; }
        private VkApiSettings VkApiSettings { get; }

        public override async Task<string> PostAsync(string _url, dynamic _data) {
            SetCallBackApiData(_data);
            return await base.PostAsync(_url, (object)_data);
        }

        public override Task<string> GetAsync(string _url, object _data) {
            SetCallBackApiData(_data);
            return base.GetAsync(_url, _data);
        }

        public override Task<string> PostJsonAsync(string _url, object _data) {
            SetCallBackApiData(_data);
            return base.PostJsonAsync(_url, _data);
        }

        private void SetCallBackApiData(dynamic _data) {
            _data.v = VkApiSettings.Version;
            _data.access_token = VkApiSettings.CallBackApiServiceToken;
        }
    }
}