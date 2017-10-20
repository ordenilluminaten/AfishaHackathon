using Newtonsoft.Json;

namespace Models.Api.VkApi.VkCallbackAPI {
    public class PrivateMessage {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("user_id")]
        public int IdUser { get; set; }
        [JsonProperty("from_id")]
        public int IdFrom { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }

    }

    public enum MessageSendErrorCode {
        UserInBlackList = 900,	//Нельзя отправлять сообщение пользователю из черного списка
        CanNotWriteFirst = 901, //Нельзя первым писать пользователю от имени сообщества.
        PrivacyRuleError = 902,	//Нельзя отправлять сообщения этому пользователю в связи с настройками приватности
        TooManyResendMessages = 913,	//Слишком много пересланных сообщений
        ToLongMessage = 914,	//Сообщение слишком длинное
    }
}
