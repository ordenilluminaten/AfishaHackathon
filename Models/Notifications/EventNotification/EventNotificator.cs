using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using Microsoft.EntityFrameworkCore;
using Models.Api.VkApi;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;
using Models.Filters;

namespace Models.Notifications.EventNotification {
    public class EventNotificator : BaseNotificator {
        public UnitOfWork<ApplicationDbContext> Unit { get; }
        public VkApi VkApi { get; }

        public EventNotificator(TimeSpan timeSpan, UnitOfWork<ApplicationDbContext> _unit, VkApi _vkApi) : base(timeSpan) {
            Unit = _unit;
            VkApi = _vkApi;
        }
        protected override async Task RunLogic(CancellationToken _cancellationToken) {
            var events = await Unit.Get<UserEvent, Guid>().GetList(new UserEventFilter {
                UseBaseFilter = false,
                UseSort = false,
                IsNotificationDate = true,
                IsUserCanRecieveNotification = true
            })
            .Select(_x => new EventNotificatorModel {
                IdUser = _x.IdUser,
                IdEvent = _x.IdEvent,
                Date = _x.Date,
                Offers = _x.Offers.Where(_y => _y.State == CompanionState.Accepted && _y.User.CanRecieveGroupMessages)
                                  .Select(_y => new EventNotificatorModel {
                                      IdUser = _y.IdUser,
                                      IdEvent = _y.UserEvent.IdEvent,
                                      Date = _y.UserEvent.Date
                                  })
            })
            .ToListAsync(_cancellationToken);

            var allEvents = new Dictionary<int, EventNotificatorModel>();
            var userEventsDict = new Dictionary<int, HashSet<int>>();

            using (var eventsEnumerator = events.GetEnumerator()) {
                while (eventsEnumerator.MoveNext()) {
                    var @event = eventsEnumerator.Current;
                    ProcessEventData(userEventsDict, allEvents, @event);
                    using (var offersEnumerator = @event.Offers.GetEnumerator()) {
                        while (offersEnumerator.MoveNext()) {
                            var offerEvent = offersEnumerator.Current;
                            ProcessEventData(userEventsDict, allEvents, offerEvent);
                        }
                    }
                }
            }
            //TODO:Смержить данные с афишей

            using (var userEvents = userEventsDict.GetEnumerator()) {
                while (userEvents.MoveNext()) {
                    var current = userEvents.Current;
                    var message = new MessageData {
                        user_id = current.Key,
                        random_id = DateTime.UtcNow.Ticks,
                        //message = GetMessageText()
                    };
                    //await VkApi.Messages.SendAsync();
                }
            }
        }

        private string GetMessageText(string eventName, DateTime date, IDictionary<int, EventNotificatorModel> userEvents) {
            var header = "[Напоминание]\n";
            var body = $"Не забудьте про мироприятие {eventName}, оно начинается {date}";
            return header + body;
        }
        private void ProcessEventData(IDictionary<int, HashSet<int>> userEventsDict,
            IDictionary<int, EventNotificatorModel> allEventsDict,
            EventNotificatorModel _data) {
            var isUserInUserEventsDict =
                userEventsDict.TryGetValue(_data.IdUser, out HashSet<int> userEventSet);

            if (allEventsDict.ContainsKey(_data.IdEvent)) {
                //Тут мероприятие уже было, поэтому надо проверить нет ли его в сет у пользователя
                if (isUserInUserEventsDict) {
                    if (!userEventSet.Contains(_data.IdEvent))
                        userEventSet.Add(_data.IdEvent);
                } else {
                    userEventsDict.Add(
                        _data.IdUser,
                        new HashSet<int> {
                            _data.IdEvent
                        });
                }
            } else {
                allEventsDict.Add(_data.IdEvent, _data);
                //Если ивента нет в общем списке ивентов и пользователь уже в словаре
                //тогда мы имеем право сразу добавить
                if (isUserInUserEventsDict) {
                    userEventSet.Add(_data.IdEvent);
                } else {
                    userEventsDict.Add(
                        _data.IdUser,
                        new HashSet<int> {
                            _data.IdEvent
                        });
                }
            }
        }
    }
}
