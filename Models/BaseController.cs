using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Models.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Models {
    public enum JsonErrorType {
        Info,
        Success,
        Warning,
        Danger
    }

    [Access]
    public abstract class BaseController : Controller {
        public IHostingEnvironment Environment { get; }
        public IMemoryCache MemoryCache { get; }
        public User CurrentUser { get; private set; }

        protected BaseController(IHostingEnvironment _environment, IHttpContextAccessor _httpContextAccessor, 
            IMemoryCache _memoryCache) {
            MemoryCache = MemoryCache;
            Environment = _environment;
        }
        public void SetCurrentUser(User _user) {
            CurrentUser = _user;
        }

        protected ActionResult JsonError(string msg, JsonErrorType type = JsonErrorType.Warning, int durationInMs = 3000) {
            return Json(new {
                error = new {
                    msg = msg,
                    type = type.ToString("G").ToLower(),
                    ms = durationInMs
                }
            });
        }

        protected IActionResult JsonModelState(ModelStateDictionary modelState) {
            var errorList = ModelState.Where(_x => _x.Value.Errors.Any()).ToDictionary(
                kvp => $"Error_{kvp.Key}",
                kvp => kvp.Value.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray()
            );

            return Json(errorList);
        }
    }
}
