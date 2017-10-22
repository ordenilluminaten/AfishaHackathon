using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Afisha;
using Models.Api.VkApi;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;
using Models.AppSettings;
using Models.Database.Tables;
using Models.Filters;
using Newtonsoft.Json.Linq;

namespace Models.Notifications.EventNotification {
    public class UserEventNotificationData {
        public int IdUser { get; set; }
        public string IdPlace { get; set; }
        public bool IsSended { get; set; }
    }
    public class EventNotificator : BaseNotificator {
        private readonly AfishaData p_afishaData;
        private readonly UnitOfWork<ApplicationDbContext> p_unit;
        private readonly VkApi p_vkApi;

        public EventNotificator(IOptions<AppSetting> _appSettings, UnitOfWork<ApplicationDbContext> _unit, VkApi _vkApi, AfishaData _afishaData) {
            Timeout = TimeSpan.FromSeconds(_appSettings.Value.AfishaBotSettings.EventNotificationLoopIntervalInSec);
            p_afishaData = _afishaData;
            p_unit = _unit;
            p_vkApi = _vkApi;
        }

        protected override async Task RunLogic(CancellationToken _cancellationToken) {
            try {
                var watch = Stopwatch.StartNew();
                var requestWatch = Stopwatch.StartNew();
                var now = DateTime.Now;

                var events = await p_unit.Get<UserEvent, Guid>().GetList(new UserEventFilter {
                    Date = null,
                    DateNow = now,
                    UseBaseFilter = false,
                    UseSort = false,
                    IsNotificationDate = true,
                    IsUserCanRecieveNotification = true
                })
                .Select(_x => new EventNotificatorModel {
                    IdUser = _x.IdUser,
                    IdPlace = _x.IdPlace,
                    Date = _x.Date,
                    Offers = _x.Offers.Where(_y => _y.State == CompanionState.Accepted && _y.User.CanRecieveGroupMessages)
                                      .Select(_y => new EventNotificatorModel {
                                          IdUser = _y.IdUser,
                                          IdPlace = _y.UserEvent.IdPlace,
                                          Date = _y.UserEvent.Date
                                      })
                })
                .ToListAsync(_cancellationToken);

                if (events.Count == 0)
                    return;

                var sendedNotifications = await p_unit.Get<UserEventNotification, Guid>()
                    .GetList(new UserEventNotificationFilter {
                        Date = now,
                        UseBaseFilter = false,
                        UseSort = false,
                        IsNotificationDate = true
                    })
                    .Select(_x => new UserEventNotificationData { IdUser = _x.IdUser, IdPlace = _x.IdPlace, IsSended = _x.IsSended })
                    .GroupBy(_x => _x.IdUser)
                    .ToDictionaryAsync(_x => _x.Key, _cancellationToken);

                requestWatch.Stop();
                var allEvents = new Dictionary<string, EventNotificatorModel>();
                var userEventsDict = new Dictionary<int, HashSet<string>>();

                using (var eventsEnumerator = events.GetEnumerator()) {
                    while (eventsEnumerator.MoveNext()) {
                        var @event = eventsEnumerator.Current;
                        if (sendedNotifications.ContainsKey(@event.IdUser)) {
                            var sendedNotification = sendedNotifications[@event.IdUser]
                                .FirstOrDefault(_x => _x.IdPlace == @event.IdPlace && _x.IsSended);
                            if (sendedNotification != null)
                                continue;
                        }
                        ProcessEventData(userEventsDict, allEvents, @event);
                        using (var offersEnumerator = @event.Offers.GetEnumerator()) {
                            while (offersEnumerator.MoveNext()) {
                                var offerEvent = offersEnumerator.Current;
                                ProcessEventData(userEventsDict, allEvents, offerEvent);
                            }
                        }
                    }
                }

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
                        var result = await p_vkApi.Messages.SendAsync(message);
                        var resultObj = JObject.Parse(result);
                        var hasError = !resultObj.TryGetValue("error", out var jToken);
                        foreach (var @event in allEvents) {
                            var newNotification = new UserEventNotification {
                                Date = DateTime.Now,
                                IdPlace = @event.Value.IdPlace,
                                IdUser = @event.Value.IdUser,
                                IsSended = hasError
                            };
                            p_unit.DbContext.Add(newNotification);
                        }
                    }
                }
                await p_unit.SaveAsync();
            } catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        private void GetMessageText(StringBuilder _stringBuilder, IEnumerable<string> eventsIdSet, IDictionary<string, EventNotificatorModel> allEvents) {
            const string finalMessage = "[Напоминание]\n";
            _stringBuilder.Append(finalMessage);
            using (var enumerator = eventsIdSet.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var @event = allEvents[enumerator.Current];
                    var place = p_afishaData.Places[@event.IdPlace];
                    if (place == null)
                        continue;
                    var body = $"У вас было запланировано событие в \"{place.Name}\" , оно начинается {@event.Date}\n";
                    _stringBuilder.Append(body);
                }
            }
        }
        private void ProcessEventData(IDictionary<int, HashSet<string>> userEventsDict,
            IDictionary<string, EventNotificatorModel> allEventsDict,
            EventNotificatorModel _data) {
            var isUserInUserEventsDict =
                userEventsDict.TryGetValue(_data.IdUser, out HashSet<string> userEventSet);

            if (allEventsDict.ContainsKey(_data.IdPlace)) {
                //Тут мероприятие уже было, поэтому надо проверить нет ли его в сет у пользователя
                if (isUserInUserEventsDict) {
                    if (!userEventSet.Contains(_data.IdPlace))
                        userEventSet.Add(_data.IdPlace);
                } else {
                    userEventsDict.Add(
                        _data.IdUser,
                        new HashSet<string> {
                            _data.IdPlace
                        });
                }
            } else {
                allEventsDict.Add(_data.IdPlace, _data);
                //Если ивента нет в общем списке ивентов и пользователь уже в словаре
                //тогда мы имеем право сразу добавить
                if (isUserInUserEventsDict) {
                    userEventSet.Add(_data.IdPlace);
                } else {
                    userEventsDict.Add(
                        _data.IdUser,
                        new HashSet<string> {
                            _data.IdPlace
                        });
                }
            }
        }
    }
}
