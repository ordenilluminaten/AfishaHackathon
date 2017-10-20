namespace Models.Api.VkApi.VkCallbackAPI.RequestDataModels {
    public class MarkAsReadData : CallBackApiData {
        public MarkAsReadData(long _idPeer, int _idStartMessage) {
            peer_id = _idPeer;
            start_message_id = _idStartMessage;
        }
        public long peer_id { get; set; }
        public int start_message_id { get; set; }
    }
}