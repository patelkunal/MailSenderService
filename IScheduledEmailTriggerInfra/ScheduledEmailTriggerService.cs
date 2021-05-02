namespace ScheduledEmailTriggerInfra
{
    using EmailCoreInfra;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public interface IScheduledEmailTriggerService
    {
        void Trigger();
    }

    public class ScheduledEmailTriggerServiceImpl : IScheduledEmailTriggerService
    {
        private readonly ILogger<ScheduledEmailTriggerServiceImpl> _logger;
        private readonly IMessageDatabaseOperations messageDatabaseOperations;
        private readonly IEmailOperations emailOperations;

        private static int numberToTrackScheduledEmails = -1;

        public ScheduledEmailTriggerServiceImpl(
            ILogger<ScheduledEmailTriggerServiceImpl> logger, IMessageDatabaseOperations messageDatabaseOperations, IEmailOperations emailOperations)
        {
            _logger = logger;
            this.messageDatabaseOperations = messageDatabaseOperations;
            this.emailOperations = emailOperations;
        }

        /// <summary>
        /// Business logic entry point for mail-sender-service
        /// <br/>
        /// <listheader>Steps to be performed</listheader>
        /// <list type="number">
        ///     <item>Fetch list of scheduled emails</item>
        ///     <item>convert model received from DB to Ses compatible models.</item>
        ///     <item>check if template exists for which email is scheduled.</item>
        ///     <item>check if template and template-data resolution happens -not necessary</item>
        ///     <item>change the state in DB - may be set to in-progress.</item>
        ///     <item>trigger emails through SES</item>
        ///     <item>update response from SES</item>
        /// </list>
        /// </summary>
        public void Trigger()
        {
            // Step #1 : Fetch list of scheduled emails
            List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> emailsFromDatabase
                = FetchDummyScheduledTemplatedEmails();

            // Step #2 : Convert model received from DB to Ses compatible models
            List<(string TemplateName, List<EmailTempalteData> EmailTempalteData, string DefaultTemplateData)> scheduledEmailData = ConvertToSesModels(emailsFromDatabase);

            if (scheduledEmailData.Count == 0)
            {
                _logger.LogInformation("There are not any scheduled emails for sending !!");
                return;
            }

            // Step #3 : check if template exists for which email is scheduled

            // Step #4 : check if template and template-data resolution happens -not necessary

            // Step #5 : change the state in DB - may be set to in-progress
            messageDatabaseOperations.UpdateState("messageId", "InProgress");


            // Step #6 : trigger emails through SES
            string messageRequestId = TriggerSesTemplateEmails(
                scheduledEmailData[0].EmailTempalteData,
                scheduledEmailData[0].TemplateName,
                scheduledEmailData[0].DefaultTemplateData
            );

            // Step #7 : update response from SES
            messageDatabaseOperations.UpdateEmailMessageTracking("messageId", messageRequestId);

            // Step #8 : change the state in DB - set to Completed
            messageDatabaseOperations.UpdateState("messageId", "Completed");
        }

        private List<(string TemplateName, List<EmailTempalteData> EmailTempalteData, string DefaultTemplateData)> ConvertToSesModels(
            List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)> EmailData, string DefaultReplacementData)> emailsFromDatabase)
        {
            return emailsFromDatabase
                        .Select(
                            emailItem => (
                                                TemplateName: emailItem.TemplateName,
                                                EmailTempalteData: emailItem.EmailData
                                                                            .Select(x =>
                                                                                        new EmailTempalteData
                                                                                        {
                                                                                            ToEmailAdresses = x.ToEmailAddresses,
                                                                                            ReplacementTemplateDataAsJson = x.ReplacementData
                                                                                        }
                                                                                    )
                                                                            .ToList(),
                                                DefaultTemplateData: emailItem.DefaultReplacementData
                                         )
                               )
                    .ToList();
        }

        /// <summary>
        /// Method/Stub to fetch scheduled emails from database.
        /// <br/>
        /// 
        /// Currently this is dummy implementation to grab fixed set of things to test infra.
        /// </summary>
        /// <returns></returns>
        private List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> FetchDummyScheduledTemplatedEmails()
        {
            Interlocked.Increment(ref numberToTrackScheduledEmails);

            List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)> scheduledEmails
                = new List<(string TemplateName, List<(List<string> ToEmailAddresses, string ReplacementData)>, string DefaultReplacementData)>();

            if (numberToTrackScheduledEmails <= 0)
            {
                scheduledEmails.AddRange(messageDatabaseOperations.FetchScheduledTemplatedEmailsV2());
            }

            return scheduledEmails;
        }


        private bool CheckTemplate(string templateName)
        {
            return false;
        }

        private string TriggerSesEmails(List<string> toEmailAddresses, string subject, string content)
        {
            string messageId = emailOperations.SendBulkEmail(toEmailAddresses, subject, content);
            return messageId;
        }

        private string TriggerSesTemplateEmails(List<EmailTempalteData> emailTempalteData,
            string templateId, string defaultTemplateDataAsJson)
        {
            return emailOperations.SendTemplatedBulkEmail(
                emailTempalteData, templateId, defaultTemplateDataAsJson
            );
        }
    }
}
