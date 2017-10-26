using Models.Api.VkApi;

namespace Models.AppSettings {
    public class AppSetting {
        public AfishaFeed AfishaFeed { get; set; }
        public AfishaBotSettings AfishaBotSettings { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public VkApiSettings VkApiSettings { get; set; }
    }
}