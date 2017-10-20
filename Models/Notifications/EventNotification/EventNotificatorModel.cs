using System;
using System.Collections.Generic;

namespace Models.Notifications.EventNotification {
    public class EventNotificatorModel {
        public int IdUser { get; set; }
        public int IdEvent { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<EventNotificatorModel> Offers { get; set; }
    }
}
