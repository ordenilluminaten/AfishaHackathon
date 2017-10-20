using System;
namespace Afisha.Controllers
{
    public class HomeController : Controllers
    {
        public IOptions<AppSetting> AppSettings { get; }
        public VkApi Api { get; }
        public UnitOfWork<ApplicationDbContext> Unit { get; }

        public HomeController(IHostingEnvironment environment,
            IHttpContextAccessor accessor,
            IMemoryCache memoryCache,
            IOptions<AppSetting> appSettings,
            UnitOfWork<ApplicationDbContext> unit)
            : base(environment, accessor, memoryCache, afishaApi)
        {
            Unit = unit;
            AppSettings = appSettings;
            Api = vkApi;
        }
    }
}
