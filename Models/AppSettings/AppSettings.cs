using Models.Api.VkApi;

namespace Models.AppSettings {
    public class AppSetting {
        public ConnectionStrings ConnectionStrings { get; set; }
        public VkApiSettings VkApiSettings { get; set; }
    }
}