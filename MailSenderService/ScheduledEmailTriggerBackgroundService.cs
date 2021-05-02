namespace MailSenderService
{
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public class ScheduledEmailTriggerBackgroundService : BackgroundService
    {
        private readonly IScheduledEmailTriggerWorker _scheduledEmailTriggerWorker;

        public ScheduledEmailTriggerBackgroundService(
            IScheduledEmailTriggerWorker scheduledEmailTriggerWorker)
        {
            _scheduledEmailTriggerWorker = scheduledEmailTriggerWorker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _scheduledEmailTriggerWorker.DoWork(stoppingToken);
        }
    }
}
