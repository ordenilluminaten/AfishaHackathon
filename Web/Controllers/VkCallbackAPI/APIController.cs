using System;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models;
using Models.Api.VkApi;
using Models.Api.VkApi.VkCallbackAPI;
using Models.AppSettings;
using Models.Extensions;

namespace Afisha.Controllers.VkCallbackAPI {

    public partial class APIController : BaseCallBackApiController {
        public APIController(IHostingEnvironment _environment, IHttpContextAccessor _httpContextAccessor,
            IMemoryCache _memoryCache, IOptions<AppSetting> _appSettings, VkApi vkApi, UnitOfWork<ApplicationDbContext> _unit) :
            base(_environment, _httpContextAccessor, _memoryCache, _appSettings) {
            VkApi = vkApi;
            Unit = _unit;
        }

        public VkApi VkApi { get; }
        public UnitOfWork<ApplicationDbContext> Unit { get; }

        [HttpPost("/callbackApi/ProcessEvent")]
        public async Task<IActionResult> ProcessEvent() {
            string str = null;
            var vkApiSettings = AppSettings.Value.VkApiSettings;
            try {
                var jObject = await Request.ReadBodyAsJObjectAsync();
                str = jObject.ToString();

                var type = (string)jObject["type"];
                var groupId = (int)jObject["group_id"];

                if (groupId != vkApiSettings.GroupId)
                    return Ok("ok");

                switch (type) {
                    case EventTypes.Confirmation: {
                            return Content(vkApiSettings.ServerVerificationMessage);
                        }
                    case EventTypes.MessageNew: {
                            var message = jObject["object"].ToObject<PrivateMessage>();
                            await ProcessNewMessage(message);
                            break;
                        }
                }

            } catch (Exception e) {
                return Ok($"error: {e.Message} | inner: {e.InnerException?.Message} | str={str}");
            }
            return Ok("ok");
        }
    }
}
