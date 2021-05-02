
namespace ScheduledEmailTriggerInfra
{
    using Microsoft.Extensions.Logging;
    using System;

    public class ScheduledEmailTriggerInfraLogger
    {
        private ILogger<ScheduledEmailTriggerInfraLogger> logger;

        public ScheduledEmailTriggerInfraLogger(ILogger<ScheduledEmailTriggerInfraLogger> logger) => this.logger = logger;

        public ILogger<ScheduledEmailTriggerInfraLogger> GetLogger() => logger;
    }
}
