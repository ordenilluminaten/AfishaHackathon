using System;
using System.Linq;
using ATMIT.Core.Web.Repository;
using System.Collections.Generic;

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
        public bool IsNotificationDate { get; set; }
        public bool? IsUserCanRecieveNotification { get; set; }

        public bool? Active { get; set; }
        public int IdEvent { get; set; }

        public override void Filter(ref IQueryable<UserEvent> list)
        {
            var now = DateTime.Now;

            if (IdEvent > 0)
                list = list.Where(_x => _x.IdEvent == IdEvent);

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