﻿using System;
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
using Models.Database.Tables;
using Models.Filters;

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

        public EventNotificator(TimeSpan timeSpan, UnitOfWork<ApplicationDbContext> _unit, VkApi _vkApi, AfishaData _afishaData) : base(timeSpan) {
            p_afishaData = _afishaData;
            p_unit = _unit;
            p_vkApi = _vkApi;
        }
        protected override async Task RunLogic(CancellationToken _cancellationToken) {
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

            var sendedNotifications = p_unit.Get<UserEventNotification, Guid>()
                .GetList(new UserEventNotificationFilter {
                    DateNow = now,
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

        private void GetMessageText(StringBuilder _stringBuilder, IEnumerable<string> eventsIdSet, IDictionary<string, EventNotificatorModel> allEvents) {
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