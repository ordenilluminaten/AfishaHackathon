namespace Models.Api.VkApi {
    public class VkApiItem {
        protected VkApiItem(VkApi _api) {
            Api = _api;
        }
        public VkApi Api { get; }
        public string RequestUrl(string _method) {
            return Api.ApiUrl + _method;
        }
    }
}