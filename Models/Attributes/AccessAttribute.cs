using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models.AppSettings;
using Models.Extensions;

namespace Models.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AccessAttribute : TypeFilterAttribute {

        public AccessAttribute() : base(typeof(AccessAttributeImpl)) { }

        public AccessAttribute(AttributeState _attributeState) : base(typeof(AccessAttributeImpl)) {
            Arguments = new object[]
            {
                _attributeState
            };
        }


        private class AccessAttributeImpl : IActionFilter {
            private readonly AttributeState p_state;
            private readonly IMemoryCache p_memoryCache;
            private readonly IHttpContextAccessor p_httpContextAccessor;
            private readonly IOptions<AppSetting> p_appSettings;
            private bool NeedToSkip(FilterContext _filterContext) {
                if (p_state == AttributeState.Ignored || _filterContext.Filters.Any(_x => _x.GetType() == typeof(AllowAnonymousFilter)))
                    return true;
                var isIgnored = false;
                byte minScope = 3;
                byte currentScope = 3;
                using (var enumerator = _filterContext.Filters.GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        if (enumerator.Current.GetType() != typeof(AccessAttributeImpl))
                            continue;
                        minScope--;
                        var currentAttr = (AccessAttributeImpl)enumerator.Current;
                        isIgnored = currentAttr.p_state == AttributeState.Ignored;
                        if (isIgnored)
                            break;
                        if (currentAttr == this)
                            currentScope = minScope;
                    }
                }
                return isIgnored || minScope < currentScope;
            }

            public AccessAttributeImpl(IOptions<AppSetting> _appSettings, IHttpContextAccessor _httpContextAccessor, IMemoryCache _memoryCache, AttributeState _attributeState = AttributeState.Respected) {
                p_appSettings = _appSettings;
                p_httpContextAccessor = _httpContextAccessor;
                p_state = _attributeState;
                p_memoryCache = _memoryCache;
            }

            public void OnActionExecuting(ActionExecutingContext _context) {
                if (NeedToSkip(_context))
                    return;

                var controller = (BaseController)_context.Controller;
                if (controller == null) {
                    AccessDenied(_context);
                    return;
                }

                var session = _context.HttpContext.Session;
                if (session.IsAvailable && session.Keys.Contains(nameof(User))) {

                    controller.SetCurrentUser(_context.HttpContext.Session.Get<User>(nameof(User)));
                    return;
                }

                var queryCollection = p_httpContextAccessor.HttpContext.Request.Query;
                if (queryCollection.Count == 0) {
                    AccessDenied(_context);
                    return;
                }

                if (!queryCollection.TryGetValue("auth_key", out var authKey) ||
                    !queryCollection.TryGetValue("api_id", out var apiId) ||
                    !queryCollection.TryGetValue("viewer_id", out var viewerId)) {
                    AccessDenied(_context);
                    return;
                }

                var authKeyString = $"{apiId}_{viewerId}_{p_appSettings.Value.VkApiSettings.ApiSecret}";
                var generatedAuthKey = authKeyString.ToMD5Hash();
                if (authKey != generatedAuthKey) {
                    AccessDenied(_context);
                }
                var user = new User {
                    Id = int.Parse(viewerId)
                };

                session.Set(nameof(User), user);
                controller.SetCurrentUser(_context.HttpContext.Session.Get<User>(nameof(User)));

            }

            public void OnActionExecuted(ActionExecutedContext _context) {
            }

            private void AccessDenied(ActionExecutingContext _context, string _redirectUrl = "/") {
                //TODO: Redirect to any page
                _context.Result = new RedirectResult(_redirectUrl);
            }
        }
    }
}
