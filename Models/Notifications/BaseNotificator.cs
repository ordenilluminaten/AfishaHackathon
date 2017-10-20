using System;
using System.Threading;
using System.Threading.Tasks;

namespace Models.Notifications {
    public abstract  class BaseNotificator {
        protected BaseNotificator(TimeSpan timeSpan) {
            Timeout = timeSpan;
        }
        private TimeSpan Timeout { get; }
        public virtual void StartProcLoop(CancellationToken _cancellationToken) {
            while (true) {
                try {
                    if (_cancellationToken.IsCancellationRequested)
                        break;
                    RunLogic(_cancellationToken);
                    if (_cancellationToken.WaitHandle.WaitOne(Timeout))
                        break;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }
        protected abstract Task RunLogic(CancellationToken _cancellationToken);
    }
}
