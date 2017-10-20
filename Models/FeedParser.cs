using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Models.AppSettings;
using ATMIT.Web.Utility;

namespace Models {
    public class FeedParser {
        private readonly Dictionary<string, PlaceType> p_typeDictionary = new Dictionary<string, PlaceType>();
        private readonly Dictionary<int, string> cityIntStringDictionary = new Dictionary<int, string>();
        private readonly Dictionary<string, int> cityStringIntDictionary = new Dictionary<string, int>();
        private readonly List<Place> p_placeList = new List<Place>();

        private readonly string p_filePath;
        private byte p_cityNameBeginIndex = 6;
        private int p_idCity = 1;

        public FeedParser(IOptions<AppSetting> _settings) {
            p_filePath = _settings.Value.AfishaFeed.Filepath;
            foreach (var type in Enum<PlaceType>.ToIEnumerable()) {
                p_typeDictionary.Add(type.ToString(), type);
            }
        }

        private void ParseXElement(XElement _element, Place _place) {
            if (_element.HasElements) {
                using (var enumerator = _element.Elements().GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        var element = enumerator.Current;
                        if (element.Name == "phone")
                            _place.Phones.Add(new Phone());
                        ParseXElement(enumerator.Current, _place);
                    }
                }
            } else {

                switch (_element.Name.LocalName) {
                    case "company-id": {
                            var str = _element.Value;
                            for (var i = 0; i < str.Length; i++) {
                                if (!char.IsDigit(str[i]))
                                    continue;
                                _place.Id = int.Parse(str.Substring(i));
                                _place.Type = p_typeDictionary[str.Substring(0, i)];
                                break;
                            }
                            break;
                        }
                    case "name": {
                            _place.Name = _element.Value;
                            break;
                        }
                    case "name-other": {
                            _place.OtherName = _element.Value;
                            break;
                        }
                    case "address": {
                            var str = _element.Value;
                            for (var i = 0; i < str.Length; i++) {
                                if (str[i] != ',')
                                    continue;
                                //-1 для учета позиции запятой
                                var cityName = str.Substring(p_cityNameBeginIndex, i - p_cityNameBeginIndex);
                                if (!cityStringIntDictionary.ContainsKey(cityName)) {
                                    cityStringIntDictionary.Add(cityName, p_idCity);
                                    _place.IdCity = p_idCity;
                                    cityIntStringDictionary.Add(p_idCity, cityName);
                                    p_idCity++;
                                } else {
                                    _place.IdCity = cityStringIntDictionary[cityName];
                                }
                                _place.Address = str;
                                break;
                            }
                            break;
                        }
                    case "country": {
                            _place.Country = _element.Value;
                            break;
                        }
                    case "url": {
                            _place.Url = _element.Value;
                            break;
                        }
                    case "add-url": {
                            _place.AddUrl = _element.Value;
                            break;
                        }
                    case "working-time": {
                            _place.WorkingTime = _element.Value;
                            break;
                        }
                    case "rating": {
                            _place.Rating = float.Parse(_element.Value.Replace('.', ','));
                            break;
                        }
                    case "number": {
                            if (_element.Parent != null && _element.Parent.Name == "phone")
                                _place.Phones[_place.Phones.Count - 1].Number = _element.Value;
                            break;
                        }
                    case "info": {
                            if (_element.Parent != null && _element.Parent.Name == "phone")
                                _place.Phones[_place.Phones.Count - 1].Info = _element.Value;
                            break;
                        }
                    case "lat": {
                            if (string.IsNullOrEmpty(_element.Value))
                                _place.Coordinate = null;
                            else
                                _place.Coordinate.Lat = double.Parse(_element.Value.Replace('.', ','));
                            break;
                        }
                    case "lon": {
                            if (_place.Coordinate == null)
                                break;
                            _place.Coordinate.Lon = double.Parse(_element.Value.Replace('.', ','));
                            break;
                        }
                    case "photo": {
                            var attr = _element.Attribute("url");
                            if (attr != null)
                                _place.Photos.Add(attr.Value);
                            break;
                        }
                }

            }
        }
        public async Task<string> Parse() {
            var watch = Stopwatch.StartNew();
            Stopwatch readWatch;
            Stopwatch loopWatch;
            string text;


            readWatch = Stopwatch.StartNew();
            text = await File.ReadAllTextAsync(p_filePath);
            var doc = XDocument.Parse(text);
            if (doc.Root == null)
                throw new NullReferenceException(nameof(doc.Root));
            readWatch.Stop();
            loopWatch = Stopwatch.StartNew();
            using (var enumerator = doc.Root.Elements("company").GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var place = new Place();
                    ParseXElement(enumerator.Current, place);
                    p_placeList.Add(place);
                }
            }
            loopWatch.Stop();
            watch.Stop();
            File.WriteAllText("time", watch.Elapsed.ToString("G"));
            return text;
        }
    }

    public class Place {
        public Place() {
            Coordinate = new Coordinate();
            Phones = new List<Phone>();
            Photos = new List<string>();
        }
        public int Id { get; set; }
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

    public class Coordinate {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class Phone {
        public string Number { get; set; }
        /// <summary>
        /// Может быть пустым
        /// </summary>
        public string Info { get; set; }
    }

    public enum PlaceType {
        Restaurant,
        ConcertHall,
        SportBuilding,
        Cinema,
        Museum,
        Theatre,
        FitnessCenter,
        Hotel,
        Shop,
        Club,
        Park,
        Gallery,
        ShowRoom
    }
}
