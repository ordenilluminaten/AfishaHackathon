using System.Threading.Tasks;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;

namespace Models.Api.VkApi {
    public class Messages : VkApiItem {
        public Messages(VkApi _api) : base(_api) {
        }
        public async Task<string> SendAsync(MessageData _messageData) {
            const string method = "messages.send";
            return await Api.PostAsync(RequestUrl(method), _messageData);
        }

        public async Task<string> MarkAsReadAsync(long _idPeer, int _idStartMessage) {
            const string method = "messages.markAsRead";
            var markAsReadData = new MarkAsReadData(_idPeer, _idStartMessage);
            return await Api.PostAsync(RequestUrl(method), markAsReadData);
        }
    }
}