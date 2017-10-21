using System;
using System.Collections.Generic;
using System.Linq;
using ATMIT.Core.Web.Repository;
using ATMIT.Web.Utility;

namespace Models.Filters
{
    public class UserEventFilter : RepositoryFilter<UserEvent, Guid>
    {
        public UserEventFilter()
        {
            SortName = nameof(UserEvent.Date);
            Sort = $"{SortName}:{SortType}";
            Active = true;
            Date = DateTime.Now.Date;
        }
        public DateTime? Date { get; set; }
        public DateTime? DateNow { get; set; }

        public bool IsNotificationDate { get; set; }
        public bool? IsUserCanRecieveNotification { get; set; }

        public bool? Active { get; set; }
        public string IdPlace { get; set; }
        public IEnumerable<int> SelectedUserIds { get; set; }

        public override void Filter(ref IQueryable<UserEvent> list)
        {
            DateTime now;
            now = DateNow ?? DateTime.Now;

            if (!IdPlace.IsNullOrWhiteSpace())
                list = list.Where(_x => _x.IdPlace == IdPlace);

            if (Active.HasValue)
                list = list.Where(_x => _x.Date > now);


            if (IsNotificationDate)
            {
                var notifyDateEnd = now.AddDays(1);
                list = list.Where(_x => now <= _x.Date && _x.Date <= notifyDateEnd);
            }

            if (IsUserCanRecieveNotification.HasValue)
            {
                list = list.Where(_x => _x.User.CanRecieveGroupMessages == IsUserCanRecieveNotification.Value);
            }

            if (Date.HasValue)
                list = list.Where(_x => _x.Date >= Date);

            if (SelectedUserIds.Count() > 0)
                list = list.Where(_x => SelectedUserIds.Contains(_x.IdUser));

            if (!string.IsNullOrEmpty(Search))
            {
                var search = Search.ToLower();
                list = list.Where(_x => _x.Comment.ToLower().Contains(search)
                    || _x.User.FirstName.ToLower().Contains(search)
                    || _x.User.LastName.ToLower().Contains(search));
            }

            base.Filter(ref list);
        }
    }
}