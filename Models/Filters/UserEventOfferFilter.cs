using System;
using System.Collections.Generic;
using System.Linq;
using ATMIT.Core.Web.Repository;
using ATMIT.Web.Utility;

namespace Models.Filters
{
    public class UserEventOfferFilter : RepositoryFilter<UserEventOffer, Guid>
    {
        public UserEventOfferFilter()
        {
            SortName = nameof(UserEvent.Date);
            Sort = $"{SortName}:{SortType}";
        }  
        public IEnumerable<int> SelectedUserIds { get; set; }

        public override void Filter(ref IQueryable<UserEventOffer> list)
        {
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