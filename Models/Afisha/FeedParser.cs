using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using ATMIT.Web.Utility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models.AppSettings;

namespace Models.Afisha {
    public class FeedParser {
        private readonly IMemoryCache p_memoryCache;
        private readonly Dictionary<string, PlaceType> p_typeDictionary = new Dictionary<string, PlaceType>();
        private readonly Dictionary<int, List<Place>> p_cityPlacesDict = new Dictionary<int, List<Place>>();
        private readonly Dictionary<int, string> p_cityIntStringDictionary = new Dictionary<int, string>();
        private readonly Dictionary<string, int> p_cityStringIntDictionary = new Dictionary<string, int>();
        private readonly Dictionary<string, Place> p_placeDict = new Dictionary<string, Place>();

        private readonly string p_filePath;
        private byte p_cityNameBeginIndex = 6;
        private int p_idCity = 1;

        public FeedParser(IOptions<AppSetting> _settings, IMemoryCache _memoryCache) {
            p_memoryCache = _memoryCache;
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
                                _place.Id = str;
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
                                if (!p_cityStringIntDictionary.ContainsKey(cityName)) {
                                    p_cityPlacesDict.Add(p_idCity, new List<Place> {
                                        _place
                                    });
                                    p_cityStringIntDictionary.Add(cityName, p_idCity);
                                    _place.IdCity = p_idCity;
                                    p_cityIntStringDictionary.Add(p_idCity, cityName);
                                    p_idCity++;
                                } else {
                                    _place.IdCity = p_cityStringIntDictionary[cityName];
                                    p_cityPlacesDict[_place.IdCity].Add(_place);
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
        public async Task Parse() {
            //try {
            var watch = Stopwatch.StartNew();
            var text = await File.ReadAllTextAsync(p_filePath);
            var doc = XDocument.Parse(text);
            if (doc.Root == null)
                throw new NullReferenceException(nameof(doc.Root));
            using (var enumerator = doc.Root.Elements("company").GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var place = new Place();
                    ParseXElement(enumerator.Current, place);
                    if (!p_placeDict.ContainsKey(place.Id))
                        p_placeDict.Add(place.Id, place);
                }
            }
            p_memoryCache.Set(AfishaData.CachePlaceDictKey, p_placeDict);
            p_memoryCache.Set(AfishaData.CacheCityDictKey, p_cityIntStringDictionary);
            p_memoryCache.Set(AfishaData.CacheCityPlacesDictKey, p_cityPlacesDict);
            watch.Stop();
            File.WriteAllText("time", watch.Elapsed.ToString("G"));
            //} catch (Exception e) {
            //    Console.WriteLine(e);
            //    throw;
            //}
        }
    }
}
