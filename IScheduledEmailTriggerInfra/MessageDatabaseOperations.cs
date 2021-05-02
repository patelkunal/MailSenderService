using EmailCoreInfra;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduledEmailTriggerInfra
{
    public interface IMessageDatabaseOperations
    {
        List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> FetchScheduledTemplatedEmailsV2();

        void UpdateState(string messageId, string updatedState);

        void UpdateEmailMessageTracking(string messageId, string messageRequestId);
    }

    public class NoOpMessageDatabaseOperationsImpl : IMessageDatabaseOperations
    {
        private readonly ILogger<IMessageDatabaseOperations> _logger;

        public NoOpMessageDatabaseOperationsImpl(ILogger<IMessageDatabaseOperations> logger)
        {
            _logger = logger;
        }

        public List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> FetchScheduledTemplatedEmailsV2()
        {
            List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> scheduledEmails
                = new List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)>();

            scheduledEmails.Add(
                    (
                        TemplateName: "Template002",
                        new List<(List<string> ToEmailAddresses, string ReplacementData)>
                        {
                            (
                                ToEmailAddresses: new List<string> { "patelkunal89@gmail.com" },
                                ReplacementData: "{ \"name\":\"Kunal\"}"
                            ),
                            (
                                ToEmailAddresses: new List<string> { "hsvaghela@gmail.com" },
                                ReplacementData: "{ \"name\":\"Hemang\"}"
                            )
                        },
                        DefaultReplacementData: "{ \"name\":\"Friend\"}"
                    )
                );

            return scheduledEmails;
        }

        public void UpdateEmailMessageTracking(string messageId, string messageRequestId)
        {
            _logger.LogInformation("Updating EmailMessage tracking for {messageId} with requestId = {messageRequestId}", messageId, messageRequestId);
        }

        public void UpdateState(string messageId, string updatedState)
        {
            _logger.LogInformation("Updating Message entity with MessageId {messageId} to state = {updatedState}", messageId, updatedState);
        }
    }
}
