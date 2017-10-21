using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Models.Database.Tables;
using Newtonsoft.Json;

namespace Models.Api.VkApi {

    public class FirstRequest {
        [FromQuery(Name = "api_result")]
        public string ApiResultJson { get; set; }

        public Lazy<ApiResult> ApiResult {
            get {
                return new Lazy<ApiResult>(() => {
                    if (string.IsNullOrEmpty(ApiResultJson))
                        return null;
                    try {
                        return JsonConvert.DeserializeObject<ApiResult>(ApiResultJson);
                    } catch (Exception) {
                        return null;
                    }
                });
            }
        }

        [FromQuery(Name = "access_token")]
        public string AccessToken { get; set; }

        [FromQuery(Name = "api_id")]
        public string ApiId { get; set; }

        [FromQuery(Name = "api_url")]
        public string ApiUrl { get; set; }

        [FromQuery(Name = "auth_key")]
        public string AuthKey { get; set; }

        [FromQuery(Name = "secret")]
        public string Secret { get; set; }

        [FromQuery(Name = "sid")]
        public string Sid { get; set; }

        [FromQuery(Name = "viewer_type")]
        public int ViewerType { get; set; }
        public CustomData CustomData { get; set; }
    }

    public class CustomData {
        public bool IsFamiliarWithBot { get; set; }

        public object Notifications { get; set; }
        public object MyPlaces { get; set; }
        public float? Longitude {get;set;}
        public float? Latitude {get;set;}
        public int? IdCity {get;set;}
    }
    public class ApiResult {
        /// <summary>
        /// Все ответы от API
        /// </summary>
        [JsonProperty("response")]
        public IList<object> Response { get; set; }

        public UserData UserData => (Response[0] as Newtonsoft.Json.Linq.JObject)?.ToObject<UserData>();
    }

    public interface IApiResponse {

    }

    public class City {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class County {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class UserData : IApiResponse {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("first_name")]
        public string Firstname { get; set; }
        [JsonProperty("last_name")]
        public string Lastname { get; set; }
        [JsonProperty("city")]
        public City City { get; set; }
        [JsonProperty("county")]
        public County County { get; set; }
        [JsonProperty("nickname")]
        public string Nickname { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("photo_200")]
        public string Photo200 { get; set; }
    }
}
