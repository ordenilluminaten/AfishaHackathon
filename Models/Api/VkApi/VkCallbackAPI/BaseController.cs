using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models.AppSettings;

namespace Models.Api.VkApi.VkCallbackAPI {

    public abstract class BaseCallBackApiController : Controller {
        protected IHostingEnvironment Environment { get; }
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected IOptions<AppSetting> AppSettings { get; }
        protected IMemoryCache MemoryCache { get; }

        protected BaseCallBackApiController(IHostingEnvironment _environment,
            IHttpContextAccessor _httpContextAccessor,
            IMemoryCache _memoryCache,
            IOptions<AppSetting> _appSettings) {

            MemoryCache = MemoryCache;
            Environment = _environment;
            HttpContextAccessor = _httpContextAccessor;
            AppSettings = _appSettings;
        }
    }
}
