using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATMIT.Core.Web.Repository;
using ATMIT.Web.Utility.Tasks;
using Microsoft.Extensions.Options;
using Models.Api.VkApi;
using Models.AppSettings;
using Models.Notifications.EventNotification;

namespace Models.Afisha.Bot {
    public class AfishaBot : IDisposable {
        public IOptions<AppSetting> AppSettings { get; }
        public UnitOfWork<ApplicationDbContext> Unit { get; }
        public VkApi VkApi { get; }
        private readonly Dictionary<BotTaskType, CancellableTask> p_tasks;
        private readonly EventNotificator p_eventNotificator;


        public AfishaBot(IOptions<AppSetting> _appSettings, EventNotificator _eventNotificator) {
            AppSettings = _appSettings;
            p_eventNotificator = _eventNotificator;
            p_tasks = new Dictionary<BotTaskType, CancellableTask>();
        }

        public void Start(BotTaskType _taskType) {
            if (p_tasks.ContainsKey(_taskType)) {
                return;
            }
            CancellableTaskProc proc;
            switch (_taskType) {
                case BotTaskType.NotificationAboutEvent:
                    proc = p_eventNotificator.StartProcLoop;
                    break;
                //case BotTaskType.NotificationAboutPartyAction:
                //    proc = p_partyActionNotificator.StartProcLoop;
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_taskType), _taskType, null);
            }
            var task = new CancellableTask(proc, TaskCreationOptions.LongRunning, $"BotTask_{_taskType}");
            p_tasks[_taskType] = task;
            task.Start();
        }
        public void Stop(BotTaskType _taskType) {
            if (!p_tasks.ContainsKey(_taskType))
                return;
            var task = p_tasks[_taskType];
            p_tasks.Remove(_taskType);
            if (task.TokenSource != null && task.TokenSource.IsCancellationRequested)
                return;
            StopTask(task);
        }

        private void StopTask(CancellableTask _task) {
            _task.Cancel();
            _task.WaitForFinished();
            _task.Dispose();
        }

        public void Dispose() {
            using (var tasks = p_tasks.GetEnumerator()) {
                while (tasks.MoveNext()) {
                    StopTask(tasks.Current.Value);
                }
            }
        }
    }
}
