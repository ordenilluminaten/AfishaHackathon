using System;
using System.Linq;
using ATMIT.Core.Web.Repository;
using Models.Afisha;

namespace Models.Filters
{
    public class PlacesFilter : RepositoryFilter<Place, string>
    {
        public PlacesFilter()
        {
            SortName = nameof(Place.Id);
            Sort = $"{SortName}:{SortType}";
        }

        public PlaceType Category { get; set; }

        public override void Filter(ref IQueryable<Place> list)
        {
            if(Category != PlaceType.All){
                list = list.Where(_x => _x.Type == Category);
            }

            if (!string.IsNullOrEmpty(Search)) {
                var search = Search.ToLower();
                list = list.Where(_x => _x.Name.ToLower().Contains(search)
                                  || _x.Address.ToLower().Contains(search));
            }

            base.Filter(ref list);
        }
    }
}
