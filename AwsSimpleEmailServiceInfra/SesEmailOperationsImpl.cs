namespace AwsSimpleEmailServiceInfra
{
    using Amazon;
    using Amazon.SimpleEmail;
    using Amazon.SimpleEmail.Model;
    using EmailCoreInfra;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SesEmailOperationsImpl : IEmailOperations
    {
        private readonly RegionEndpoint RegionEndpoint;

        private readonly string FromEmailAddress;

        private readonly string ConfigurationSetName;

        private readonly ILogger<SesEmailOperationsImpl> _logger;

        public SesEmailOperationsImpl(string fromEmailAddress,
            string configSetName, RegionEndpoint regionEndpoint,
            ILogger<SesEmailOperationsImpl> logger = null)
        {
            FromEmailAddress = fromEmailAddress;
            ConfigurationSetName = configSetName;
            RegionEndpoint = regionEndpoint;
            _logger = logger;
        }

        public string SendBulkEmail(List<string> toEmailAddresses, string subject, string content)
        {
            return SendEmailInternal(toEmailAddresses, subject, content);
        }

        public string SendEmail(string toEmailAddress, string subject, string content)
        {
            return SendEmailInternal(new List<string> { toEmailAddress }, subject, content);
        }

        public string SendTemplatedBulkEmail(List<EmailTempalteData> emailTempalteData,
            string templateId, string defaultTemplateDataAsJson)
        {
            return SendTemplatedEmailInternal(emailTempalteData, templateId, defaultTemplateDataAsJson);
        }

        private string SendTemplatedEmailInternal(List<EmailTempalteData> emailTempalteData, string templateId, string defaultTemplateDataAsJson)
        {
            using (AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(RegionEndpoint))
            {
                SendBulkTemplatedEmailRequest sendBulkTemplatedEmailRequest = new SendBulkTemplatedEmailRequest
                {
                    Source = FromEmailAddress,
                    Template = templateId,
                    Destinations = emailTempalteData
                        .Select(x => new BulkEmailDestination { Destination = new Destination(x.ToEmailAdresses), ReplacementTemplateData = x.ReplacementTemplateDataAsJson })
                        .ToList(),
                    DefaultTemplateData = defaultTemplateDataAsJson,
                    ConfigurationSetName = ConfigurationSetName
                };

                try
                {
                    _logger.LogInformation("Sending email using Amazon SES...");

                    SendBulkTemplatedEmailResponse response =
                        client.SendBulkTemplatedEmailAsync(sendBulkTemplatedEmailRequest).Result;

                    _logger.LogInformation($"HttpStatusCode = {response.HttpStatusCode}, MessageId = {string.Join(",", response.Status.Select(x => $"{x.MessageId}|{x.Status}").ToArray())}, RequestId = {response.ResponseMetadata.RequestId}, Metadata = {string.Join("; ", response.ResponseMetadata.Metadata)}");
                    _logger.LogInformation("The email was sent successfully.");
                    return string.Join(",", response.Status.Select(x => $"{x.MessageId}|{x.Status}").ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError("The email was not sent.");
                    _logger.LogError("Error message: " + ex.Message);
                    throw ex;
                }
            }
        }

        private string SendEmailInternal(List<string> toEmailAddresses, string subject, string content)
        {
            using (AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(RegionEndpoint))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = FromEmailAddress,
                    Destination = new Destination { ToAddresses = toEmailAddresses },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body { Text = new Content { Data = content, Charset = "UTF-8" } }
                    },
                    ConfigurationSetName = ConfigurationSetName
                };

                try
                {
                    _logger.LogInformation("Sending email using Amazon SES...");
                    SendEmailResponse response = client.SendEmailAsync(sendRequest).Result;
                    _logger.LogInformation($"HttpStatusCode = {response.HttpStatusCode}, MessageId = {response.MessageId}, RequestId = {response.ResponseMetadata.RequestId}, Metadata = {string.Join(";", response.ResponseMetadata.Metadata)}");
                    _logger.LogInformation("The email was sent successfully.");
                    return response.MessageId;
                }
                catch (Exception ex)
                {
                    _logger.LogError("The email was not sent.");
                    _logger.LogError("Error message: " + ex.Message);
                    throw ex;
                }
            }
        }
    }
}