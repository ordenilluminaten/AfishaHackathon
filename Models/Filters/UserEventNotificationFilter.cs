using System;
using System.Linq;
using ATMIT.Core.Web.Repository;
using Models.Database.Tables;

namespace Models.Filters {
    public class UserEventNotificationFilter : RepositoryFilter<UserEventNotification, Guid> {
        public UserEventNotificationFilter() {
            SortName = nameof(UserEvent.Date);
            Sort = $"{SortName}:{SortType}";
        }
        public bool IsNotificationDate { get; set; }
        public DateTime? Date { get; set; }


        public override void Filter(ref IQueryable<UserEventNotification> list) {
            DateTime now;
            now = Date ?? DateTime.Now;

            if (IsNotificationDate) {
                var notifyDateEnd = now.AddDays(1);
                now = now.AddDays(-1);
                list = list.Where(_x => now <= _x.Date && _x.Date <= notifyDateEnd);
            }

            base.Filter(ref list);
        }
    }
}
