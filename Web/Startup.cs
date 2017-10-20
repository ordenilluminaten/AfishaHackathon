using System;
using System.Linq;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using ATMIT.Web.Utility.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Models;
//using Models.Api.AfishaApi;
using Models.Api.VkApi;
//using Models.Api.VkApi.Bot;
using Models.AppSettings;
using Models.Extensions;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Afisha
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            ContextFactory.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlServer()
            .AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseSqlServer(ContextFactory.ConnectionString)
                       .UseInternalServiceProvider(serviceProvider));

            if (!Env.IsDevelopment())
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });
                services.Configure<RewriteOptions>(options =>
                {
                    options.AddRedirectToHttps();
                });
            }
            //var test = new EventNotificator(TimeSpan.FromSeconds(5));
            //CancellableTaskProc proc = test.StartProcLoop;
            //var task = new CancellableTask(proc, TaskCreationOptions.LongRunning, $"BotTask");
            //task.Start();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
            });
            services.AddTransient<UnitOfWork<ApplicationDbContext>>();
            services.Configure<AppSetting>(Configuration);
            //services.AddSingleton<AfishaApi>();
            services.AddSingleton<VkApi>();
            //services.AddSingleton<AfishaBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<AppSetting> _appSetting)
        {

            if (env.IsDevelopment())
            {
                //_afishaBot.Start(BotTaskType.NotificationAboutEvent);
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            app.UseStaticFiles();
            //app.UseErrorHandler();
            app.UseSession();
            //обновляем миграции
            if (env.IsProduction())
                app.UseMigrations();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}");
            });
        }
    }
}
