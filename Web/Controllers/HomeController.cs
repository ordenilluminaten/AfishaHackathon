using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Models;
using Models.Afisha;
using Models.Api.VkApi;
using Models.AppSettings;
using Models.Extensions;
using Models.Filters;
using ATMIT.Web.Utility;
using Models.Api.VkApi.VkCallbackAPI.RequestDataModels;
using Models.Database.Tables;

namespace Afisha.Controllers {
    public class HomeController : BaseController {
        public IOptions<AppSetting> AppSettings { get; }
        public VkApi Api { get; }
        public UnitOfWork<ApplicationDbContext> Unit { get; }
        public AfishaData Afisha { get; set; }

        public class UserPlaces {
            public Guid Id { get; set; }
            public string IdPlace { get; set; }
            public DateTime Date { get; set; }
            public int UserTotalCount { get; set; }
            public string Title { get; set; }
            public bool IsOffer { get; set; }
        }


        public HomeController(IHostingEnvironment environment,
            IHttpContextAccessor accessor,
            IMemoryCache memoryCache,
            IOptions<AppSetting> appSettings,
            VkApi vkApi,
            UnitOfWork<ApplicationDbContext> unit,
            AfishaData afisha)
            : base(environment, accessor, memoryCache) {
            Unit = unit;
            AppSettings = appSettings;
            Api = vkApi;
            Afisha = afisha;
        }

        /// <summary>
        /// Полный запрос от ВК
        /// http://localhost:50694/?api_url=https://api.vk.com/api.php&api_id=6206497&api_settings=1&viewer_id=46611989&viewer_type=4&sid=83876255d0fa6f12ed3a2f0d9005692df68ca0348dd1dfbe4d0d52c86ae6ecd01d56b92328a89486bf72f&secret=83d669486c&access_token=bd07a6fc28d0d8491e122703b7090996d2c8b7d5ee52fbcef603ae67d5edc5bb4166effc81fa3b840e602&user_id=0&is_app_user=1&auth_key=972a517ae2939e02e9b3834c97da5a50&language=0&parent_language=0&is_secure=1&ads_app_id=6206497_f18e3a738302567922&api_result=%7B%22response%22%3A%5B%7B%22id%22%3A46611989%2C%22first_name%22%3A%22%D0%9C%D0%B8%D1%88%D0%B0%22%2C%22last_name%22%3A%22%D0%A8%D1%82%D0%B0%D0%BD%D1%8C%D0%BA%D0%BE%22%2C%22nickname%22%3A%22BenyKrik%22%2C%22city%22%3A%7B%22id%22%3A2%2C%22title%22%3A%22%D0%A1%D0%B0%D0%BD%D0%BA%D1%82-%D0%9F%D0%B5%D1%82%D0%B5%D1%80%D0%B1%D1%83%D1%80%D0%B3%22%7D%2C%22country%22%3A%7B%22id%22%3A1%2C%22title%22%3A%22%D0%A0%D0%BE%D1%81%D1%81%D0%B8%D1%8F%22%7D%2C%22photo_200%22%3A%22https%3A%5C%2F%5C%2Fpp.userapi.com%5C%2Fc639129%5C%2Fv639129989%5C%2F2ba7f%5C%2FLGwnKIkG1pw.jpg%22%2C%22has_mobile%22%3A1%2C%22online%22%3A0%2C%22status%22%3A%22%D0%AD%D0%B9%2C%20%D0%BF%D1%80%D0%BE%D1%81%D0%BD%D0%B8%D1%81%D1%8C%21%20%D0%9D%D1%83%20%D1%82%D1%8B%20%D0%B8%20%D1%81%D0%BE%D0%BD%D1%8F.%20%D0%A2%D0%B5%D0%B1%D1%8F%20%D0%B4%D0%B0%D0%B6%D0%B5%20%D0%B2%D1%87%D0%B5%D1%80%D0%B0%D1%88%D0%BD%D0%B8%D0%B9%20%D1%88%D1%82%D0%BE%D1%80%D0%BC%20%D0%BD%D0%B5%20%D1%80%D0%B0%D0%B7%D0%B1%D1%83%D0%B4%D0%B8%D0%BB.%20%D0%93%D0%BE%D0%B2%D0%BE%D1%80%D1%8F%D1%82%2C%20%D0%BC%D1%8B%20%D1%83%D0%B6%D0%B5%20%D0%BF%D1%80%D0%B8%D0%BF%D0%BB%D1%8B%D0%BB%D0%B8%20%D0%B2%20%D0%9C%D0%BE%D1%80%D1%80%D0%BE%D0%B2%D0%B8%D0%BD%D0%B4.%20%D0%9D%D0%B0%D1%81%20%D0%B2%D1%8B%D0%BF%D1%83%D1%81%D1%82%D1%8F%D1%82%2C%20%D1%8D%D1%82%D0%BE%20%D1%82%D0%BE%D1%87%D0%BD%D0%BE%21%22%7D%5D%7D&referrer=group&lc_name=135b0cdd&sign=c76f449244e9deac48004ec57e83a275eb2c2f8ea159a8c85fb807ca3dc25457&hash
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(FirstRequest request) {
            //TODO обработка запроса
            if (Environment.IsDevelopment()) {
                if (string.IsNullOrEmpty(request.ApiResultJson))
                    request.ApiResultJson = @"{'response':[{'id':46611989,'first_name':'Миша','last_name':'Штанько','nickname':'BenyKrik','city':{'id':2,'title':'Санкт - Петербург'},'country':{'id':1,'title':'Россия'},'photo_200':'https:\/\/ pp.userapi.com\/ c639129\/ v639129989\/ 2ba7f\/ LGwnKIkG1pw.jpg','has_mobile':1,'online':0,'status':'Эй, проснись!Ну ты и соня.Тебя даже вчерашний шторм не разбудил. Говорят, мы уже приплыли в Морровинд.Нас выпустят, это точно!'}]}";

            }
            //обновляем юзера, await можно убрать (наверное)
            await Task.Run(async () => {
                var userData = request.ApiResult.Value.UserData;
                using (var ctx = ContextFactory.Create()) {
                    var user = await ctx.Users.FirstOrDefaultAsync(_x => _x.Id == userData.Id);
                    if (user == null) {
                        user = new User();
                        await ctx.Users.AddAsync(user);
                    }
                    user.Id = userData.Id;
                    user.Avatar = userData.Photo200;
                    user.FirstName = userData.Firstname;
                    user.LastName = userData.Lastname;
                    user.LastEnter = DateTime.Now;
                    CurrentUser.SetUserData(user);
                    HttpContext.Session.Set(nameof(User), CurrentUser);
                    await ctx.SaveChangesAsync();
                }
            });

            request.CustomData = new CustomData();
            request.CustomData.IsFamiliarWithBot = CurrentUser.IsFamiliarWithBot;
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Places(PlacesFilter filter) {
            filter.UseSort = false;
            var list = Afisha.CityPlaces[1].OrderByDescending(_x => _x.Photos.Count());
            var q = list.AsQueryable();
            filter.Filter(ref q);

            return Json(new {
                Items = q,
                Filter = filter
            });
        }

        [HttpPost]
        public async Task<IActionResult> UserEvents(UserEventFilter filter) {
            var items = await Unit.Get<UserEvent, Guid>().GetList(filter)
                .Include(x => x.Offers)
                    .ThenInclude(x => x.User)
                .Include(x => x.User)
                .ToArrayAsync();
            return Json(
                new {
                    Items = items,
                    Filter = filter
                });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOffer(Guid idUserEvent) {
            var userEvent = await Unit.Get<UserEvent>().FindAsync(_x => _x.Id == idUserEvent,
                _x => _x.Include(_y => _y.Offers));
            if (userEvent == null)
                return JsonError("Событие на найдено");
            if (userEvent.UserCount == userEvent.Offers.Count)
                return JsonError("В данном событии сейчас нет свободных мест");
            if (userEvent.Offers.Any(_x => _x.IdUser == CurrentUser.Id))
                return JsonError("Вы уже состоите в данном событии");
            userEvent.Offers.Add(new UserEventOffer {
                IdUserEvent = idUserEvent,
                Date = DateTime.Now,
                IdUser = CurrentUser.Id,
                State = CompanionState.Pending
            });
            await Unit.SaveAsync();
            if (!CurrentUser.CanRecieveGroupMessages)
                return Json(true);

            var newUserNotification = new UserNotification {
                IdUser = userEvent.IdUser,
                IdUserEvent = userEvent.Id,
                IdUserFrom = CurrentUser.Id,
                Type = UserNotificationType.NewOffer,
                Date = DateTime.Now
            };

            Unit.DbContext.UserNotifications.Add(newUserNotification);
            await Unit.SaveAsync();

            var newMessageData = new MessageData {
                random_id = DateTime.Now.Ticks,
                user_id = userEvent.IdUser,
                message = $"Пользователь {CurrentUser.FullName} хочет пойти с Вами в" +
                          $" \"{Afisha.Places[userEvent.IdPlace].Name}\"\n" +
                          $"Перейти: {AppSettings.Value.VkApiSettings.AppUrl}"
            };

            await Api.Messages.SendAsync(newMessageData);

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveOffer(Guid idOffer) {
            var offer = await Unit.Get<UserEventOffer>().FindAsync(_x => _x.Id == idOffer);
            if (offer == null)
                return JsonError("Заявка на найдена");
            if (offer.IdUser != CurrentUser.Id)
                return JsonError("Нет доступа к событию");

            Unit.Get<UserEventOffer>().Delete(offer);
            await Unit.SaveAsync();
            return Json(true);
        }

        [HttpPost]
        [Route(nameof(GetUsersEventsByIds))]
        public async Task<IActionResult> GetUsersEventsByIds(int[] ids) {
            var userDict = new Dictionary<int, List<UserPlaces>>();
            var userEvents = await Unit.Get<UserEvent, Guid>().All
                .Where(_x => ids.Contains(_x.IdUser))
                .ToArrayAsync();

            var userEventOffers = await Unit.Get<UserEventOffer, Guid>()
            .All
            .Where(_x => ids.Contains(_x.IdUser))
            .Include(_x => _x.UserEvent)
            .ToArrayAsync();

            foreach (var userEvent in userEvents) {
                List<UserPlaces> events = null;
                if (!userDict.TryGetValue(userEvent.IdUser, out events)) {
                    events = new List<UserPlaces>();
                    userDict[userEvent.IdUser] = events;
                }
                events.Add(new UserPlaces {
                    Id = userEvent.Id,
                    IdPlace = userEvent.IdPlace,
                    Date = userEvent.Date,
                    UserTotalCount = userEvent.UserCount,
                    Title = Afisha.Places[userEvent.IdPlace].Name
                });
            }

            foreach (var userEventOffer in userEventOffers) {
                List<UserPlaces> events = null;
                if (!userDict.TryGetValue(userEventOffer.IdUser, out events)) {
                    events = new List<UserPlaces>();
                    userDict[userEventOffer.IdUser] = events;
                }
                events.Add(new UserPlaces {
                    Id = userEventOffer.Id,
                    IdPlace = userEventOffer.UserEvent.IdPlace,
                    Date = userEventOffer.Date,
                    UserTotalCount = userEventOffer.UserEvent.UserCount,
                    Title = Afisha.Places[userEventOffer.UserEvent.IdPlace].Name,
                    IsOffer = true
                });
            }

            return Json(new {
                items = userDict,
                count = userDict.Count
            });
        }

        [HttpGet]
        [Route(nameof(CreateInviteCompanion))]
        public async Task<IActionResult> CreateInviteCompanion(string idPlace) {
            return PartialView("InviteCompanionModal", new UserEvent {
                IdPlace = idPlace,
                Date = DateTime.Now
            });
        }



        [HttpPost]
        [Route(nameof(CreateInviteCompanion))]
        public async Task<IActionResult> CreateInviteCompanion(UserEvent invite) {
            ModelState.RemoveFor<UserEvent>(_x => _x.Id);
            ModelState.RemoveFor<UserEvent>(_x => _x.IdUser);
            if (!ModelState.IsValid)
                return JsonModelState(ModelState);

            invite.IdUser = CurrentUser.Id;
            Unit.Get<UserEvent>().Create(invite);
            await Unit.SaveAsync();

            return Json(new {
                idUserEvent = invite.Id
            });
        }

        [HttpPost]
        public async Task SetFamiliarWithBot() {
            var dbUser = await Unit.Get<User>().FindAsync(_x => _x.Id == CurrentUser.Id);
            if (dbUser == null)
                throw new NullReferenceException(nameof(dbUser));
            dbUser.IsFamiliarWithBot = true;
            await Unit.SaveAsync();
        }
    }
}

