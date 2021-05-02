namespace EmailCoreInfra
{
    using System.Collections.Generic;

    public interface IEmailOperations
    {
        string SendEmail(string toEmailAddress, string subject, string content);

        string SendBulkEmail(List<string> toEmailAddresses, string subject, string content);

        string SendTemplatedBulkEmail(List<EmailTempalteData> emailTempalteData,
            string templateId, string defaultTemplateDataAsJson);
    }
}
