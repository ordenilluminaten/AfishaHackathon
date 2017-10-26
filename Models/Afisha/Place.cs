using System.Collections.Generic;
using ATMIT.Web.Utility.Data;

namespace Models.Afisha {
    public class Place : IEntity {
        public Place() {
            Coordinate = new Coordinate();
            Phones = new List<Phone>();
            Photos = new List<string>();
        }
        public string Id { get; set; }
        public PlaceType Type { get; set; }
        public int IdCity { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string OtherName { get; set; }
        public string Country { get; set; }
        /// <summary>
        /// Поле может быть пустым
        /// </summary>
        public string Url { get; set; }
        public string AddUrl { get; set; }
        public string WorkingTime { get; set; }
        public float Rating { get; set; }
        public Coordinate Coordinate { get; set; }
        public List<Phone> Phones { get; set; }
        public List<string> Photos { get; set; }
    }
}