using System;
using ATMIT.Core.Web.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models;
using Models.Api.VkApi;
using Models.AppSettings;

namespace Afisha.Controllers
{
    public class HomeController : BaseController
    {
        public IOptions<AppSetting> AppSettings { get; }
        public VkApi Api { get; }
        public UnitOfWork<ApplicationDbContext> Unit { get; }

        public HomeController(IHostingEnvironment environment,
            IHttpContextAccessor accessor,
            IMemoryCache memoryCache,
            IOptions<AppSetting> appSettings,
            VkApi vkApi,
            UnitOfWork<ApplicationDbContext> unit)
            : base(environment, accessor, memoryCache)
        {
            Unit = unit;
            AppSettings = appSettings;
            Api = vkApi;
        }
    }
}
