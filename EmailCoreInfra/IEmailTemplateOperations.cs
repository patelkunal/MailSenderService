namespace EmailCoreInfra
{
    public interface IEmailTemplateOperations
    {
        void GetEmailTemplate(string templateName);

        void CreateOrUpdateEmailTemplate(string templateName, string subject, string text, string html);

        void DeleteEmailTemplate(string templateName);
    }
}
