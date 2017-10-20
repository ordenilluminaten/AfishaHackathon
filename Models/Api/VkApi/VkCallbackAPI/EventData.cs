using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Models.Api.VkApi.VkCallbackAPI {
    public class EventData {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("object")]
        public string JsonObject { get; set; }
        [JsonProperty("group_id")]
        public int IdGroup { get; set; }
    }
}
