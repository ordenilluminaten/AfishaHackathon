using System;
using System.Threading.Tasks;
using ATMIT.Web.Utility;
using Models;
using Models.Afisha.Bot;
using Models.Api.VkApi.VkCallbackAPI;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;

namespace Afisha.Controllers.VkCallbackAPI {

    public partial class APIController {
        public async Task<bool> ProcessNewMessage(PrivateMessage _message) {
            if (_message.Body.IsNullOrWhiteSpace()) {
                return false;
            }
            var splittedBody = _message.Body.Split(" ");
            if (splittedBody.Length == 0 || splittedBody[0].ToLower() != "бот")
                return false;
            //messages.send
            await VkApi.Messages.MarkAsReadAsync(_message.IdUser, _message.Id);
            var messageData = new MessageData {
                user_id = _message.IdUser,
                random_id = DateTime.UtcNow.Ticks
            };
            if (_message.Body.Trim().ToLower() != BotCommands.Start) {
                messageData.message = "Извините, но сейчас я знаю только слово \"бот старт\".";
                await VkApi.Messages.SendAsync(messageData);
            } else {
                var user = await Unit.Get<User>().FindAsync(_x => _x.Id == _message.IdUser);
                if (user == null) {
                    messageData.message = "Извините, произошла ошибка.";
                    await VkApi.Messages.SendAsync(messageData);
                } else {
                    user.CanRecieveGroupMessages = true;
                    await Unit.SaveAsync();
                    messageData.message = "Оповещения успешно включены, спасибо.";
                    await VkApi.Messages.SendAsync(messageData);
                }
            }
            return true;
        }
    }
}
