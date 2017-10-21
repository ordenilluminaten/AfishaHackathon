using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using Microsoft.EntityFrameworkCore;
using Models.Afisha;
using Models.Api.VkApi;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;
using Models.Filters;

namespace Models.Notifications.EventNotification {
    public class EventNotificator : BaseNotificator {
        private readonly AfishaData p_afishaData;
        private readonly UnitOfWork<ApplicationDbContext> p_unit;
        private readonly VkApi p_vkApi;

        public EventNotificator(TimeSpan timeSpan, UnitOfWork<ApplicationDbContext> _unit, VkApi _vkApi, AfishaData _afishaData) : base(timeSpan) {
            p_afishaData = _afishaData;
            p_unit = _unit;
            p_vkApi = _vkApi;
        }
        protected override async Task RunLogic(CancellationToken _cancellationToken) {
            var watch = Stopwatch.StartNew();
            var requestWatch = Stopwatch.StartNew();
            var events = await p_unit.Get<UserEvent, Guid>().GetList(new UserEventFilter {
                Date = null,
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

            if (events.Count == 0)
                return;

            //var sendedNotifications = Unit.Get<UserEventNotification, Guid>().All.Where(_x=>_x.)

            requestWatch.Stop();
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
            var stringBuilder = new StringBuilder();
            using (var userEvents = userEventsDict.GetEnumerator()) {
                while (userEvents.MoveNext()) {
                    var current = userEvents.Current;
                    GetMessageText(stringBuilder, current.Value, allEvents);
                    var message = new MessageData {
                        user_id = current.Key,
                        random_id = DateTime.UtcNow.Ticks,
                        message = stringBuilder.ToString()
                    };
                    stringBuilder.Clear();
                    await p_vkApi.Messages.SendAsync(message);
                }
            }
            watch.Stop();
            watch.Reset();
        }

        private void GetMessageText(StringBuilder _stringBuilder, IEnumerable<int> eventsIdSet, IDictionary<int, EventNotificatorModel> allEvents) {
            const string finalMessage = "[Напоминание]\n";
            _stringBuilder.Append(finalMessage);
            using (var enumerator = eventsIdSet.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var @event = allEvents[enumerator.Current];
                    //var place = p_afishaData.Places[@event.IdEvent];
                    var place = string.Empty;
                    if (place == null)
                        continue;
                    var body = $"Не забудьте про мероприятие {place}, оно начинается {@event.Date}\n";
                    _stringBuilder.Append(body);
                }
            }
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
