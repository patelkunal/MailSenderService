namespace MailSenderService
{
    using Microsoft.Extensions.Logging;
    using ScheduledEmailTriggerInfra;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IScheduledEmailTriggerWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }

    public class ScheduledEmailTriggerWorker : IScheduledEmailTriggerWorker
    {
        private readonly ILogger<ScheduledEmailTriggerWorker> _logger;
        private readonly IScheduledEmailTriggerService _scheduledEmailTriggerService;
        private readonly int _delayInSeconds;

        public ScheduledEmailTriggerWorker(ILogger<ScheduledEmailTriggerWorker> logger,
            IScheduledEmailTriggerService scheduledEmailService, int delayInSeconds = 60)
        {
            _logger = logger;
            _scheduledEmailTriggerService = scheduledEmailService;
            _delayInSeconds = delayInSeconds;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("ScheduledEmailTriggerWorker has woken up to do work !!");

                // Do actual work
                _scheduledEmailTriggerService.Trigger();
                // Completed actual work

                _logger.LogInformation($"ScheduledEmailTriggerWorker wiil sleep for {_delayInSeconds} seconds");
                await Task.Delay(1000 * _delayInSeconds);
                _logger.LogInformation($"ScheduledEmailTriggerWorker completed sleeping for {_delayInSeconds} seconds");
            }
        }
    }
}
