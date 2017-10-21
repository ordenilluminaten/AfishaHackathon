using System;
using System.Threading.Tasks;
using ATMIT.Web.Utility;
using Models;
using Models.Afisha.Bot;
using Models.Api.VkApi;
using Models.Api.VkApi.VkCallbackAPI;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;

namespace Afisha.Controllers.VkCallbackAPI {


    public partial class APIController {
        private async Task SetToggleNotification(MessageData _messageData, PrivateMessage _message, bool _canRecieveGroupMessages) {
            var user = await Unit.Get<User>().FindAsync(_x => _x.Id == _message.IdUser);
            if (user == null) {
                _messageData.message = "Извините, произошла ошибка.";
                await VkApi.Messages.SendAsync(_messageData);
            } else {
                user.CanRecieveGroupMessages = _canRecieveGroupMessages;
                await Unit.SaveAsync();
                _messageData.message = "Оповещения успешно включены, спасибо.";
                await VkApi.Messages.SendAsync(_messageData);
            }
        }
        public async Task<bool> ProcessNewMessage(PrivateMessage _message) {
            if (_message.Body.IsNullOrWhiteSpace()) {
                return false;
            }

            var splittedBody = _message.Body.Split(" ");

            if (splittedBody.Length == 0 || splittedBody[0].ToLower() != "бот")
                return false;

            await VkApi.Messages.MarkAsReadAsync(_message.IdUser, _message.Id);
            var messageData = new MessageData {
                user_id = _message.IdUser,
                random_id = DateTime.UtcNow.Ticks
            };

            var messageBody = _message.Body.Trim().ToLower();

            switch (messageBody) {
                case BotCommands.Stop: {
                        await SetToggleNotification(messageData, _message, true);
                        break;
                    }
                case BotCommands.Start: {
                        await SetToggleNotification(messageData, _message, false);
                        break;
                    }
                case BotCommands.Bot:
                default: {
                        messageData.message = "Допустимые команды:\n" +
                            "\"Бот\" - список команд бота\n" +
                            "\"Бот старт\" - подписка на оповещения бота\n" +
                            "\"Бот стоп\"- отписка от оповещений бота\n";
                        await VkApi.Messages.SendAsync(messageData);
                        break;
                    }
            }
            return true;
        }
    }
}
