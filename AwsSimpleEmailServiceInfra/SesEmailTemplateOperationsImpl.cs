namespace AwsSimpleEmailServiceInfra
{
    using Amazon;
    using Amazon.SimpleEmail;
    using Amazon.SimpleEmail.Model;
    using EmailCoreInfra;
    using Microsoft.Extensions.Logging;
    using System;

    public class SesEmailTemplateOperationsImpl : IEmailTemplateOperations
    {
        private readonly RegionEndpoint _regionEndpoint;

        private readonly ILogger<SesEmailTemplateOperationsImpl> _logger;

        public SesEmailTemplateOperationsImpl(RegionEndpoint regionEndpoint,
            ILogger<SesEmailTemplateOperationsImpl> logger)
        {
            _regionEndpoint = regionEndpoint;
            _logger = logger;
        }

        public void CreateOrUpdateEmailTemplate(string templateName, string subject, string text, string html)
        {
            using (AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(_regionEndpoint))
            {
                Template template = new Template
                {
                    TemplateName = templateName,
                    SubjectPart = subject,
                    TextPart = text,
                    HtmlPart = html
                };
                CreateTemplateRequest createTemplateRequest = new CreateTemplateRequest { Template = template };
                try
                {
                    _logger.LogInformation($"CreateOrUpdateEmailTemplate | TemplateName = {createTemplateRequest.Template.TemplateName}");
                    CreateTemplateResponse createTemplateResponse = client.CreateTemplateAsync(createTemplateRequest).Result;
                    _logger.LogInformation($"CreateTemplateResponse.StatusCode = {createTemplateResponse.HttpStatusCode}");
                    _logger.LogInformation($"CreateTemplateResponse.RequestId = {createTemplateResponse.ResponseMetadata.RequestId}");
                    _logger.LogInformation($"CreateTemplateResponse.Metadata = {string.Join("; ", createTemplateResponse.ResponseMetadata.Metadata)}");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.GetType() == typeof(AlreadyExistsException))
                    {
                        _logger.LogInformation($"Template already exists, updating template content for {templateName}");
                        try
                        {
                            UpdateTemplateResponse updateTemplateResponse = client.UpdateTemplateAsync(new UpdateTemplateRequest { Template = template }).Result;
                            _logger.LogInformation($"UpdateTemplateResponse.StatusCode = {updateTemplateResponse.HttpStatusCode}");
                            _logger.LogInformation($"UpdateTemplateResponse.RequestId = {updateTemplateResponse.ResponseMetadata.RequestId}");
                            _logger.LogInformation($"UpdateTemplateResponse.Metadata = {string.Join("; ", updateTemplateResponse.ResponseMetadata.Metadata)}");
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else
                    {
                        _logger.LogError("Failure in CreateOrUpdateTemplate. Error message: " + ex.Message);
                        throw ex;
                    }
                }
            }
        }

        public void DeleteEmailTemplate(string templateName)
        {
            using (AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(_regionEndpoint))
            {
                try
                {
                    _logger.LogInformation($"DeleteEmailTemplate | TemplateName = {templateName}");
                    DeleteTemplateResponse result = client.DeleteTemplateAsync(new DeleteTemplateRequest { TemplateName = templateName }).Result;
                    _logger.LogInformation($"DeleteTemplateResponse.StatusCode = {result.HttpStatusCode}");
                    _logger.LogInformation($"DeleteTemplateResponse.RequestId = {result.ResponseMetadata.RequestId}");
                    _logger.LogInformation($"DeleteTemplateResponse.Metadata = {string.Join("; ", result.ResponseMetadata.Metadata)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failure in DeleteEmailTemplate. Error message: " + ex.Message);
                    throw ex;
                }
            }
        }

        public void GetEmailTemplate(string templateName)
        {
            using (AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(_regionEndpoint))
            {
                try
                {
                    GetTemplateResponse getTemplateResponse = client.GetTemplateAsync(new GetTemplateRequest { TemplateName = templateName }).Result;
                    _logger.LogInformation($"GetTemplateResponse.HttpStatusCode = {getTemplateResponse.HttpStatusCode}");
                    _logger.LogInformation($"GetTemplateResponse.Template.TemplateName = {getTemplateResponse.Template.TemplateName}");
                    _logger.LogInformation($"GetTemplateResponse.Template.Subject = {getTemplateResponse.Template.SubjectPart}");
                    _logger.LogInformation($"GetTemplateResponse.Template.TextPart = {getTemplateResponse.Template.TextPart}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failure while fetching template {templateName}, Error Message = " + e.Message);
                    _logger.LogError($"Failure while fetching template {templateName}, e.InnerException.GetType() = {e.InnerException.GetType()}");

                    if (e.InnerException.GetType() == typeof(TemplateDoesNotExistException))
                    {
                        _logger.LogError($"Failure while fetching template {templateName}, Template doesn't exist, Error Message = " + e.Message);
                        throw e.InnerException;
                    }

                    throw e;
                }
            }
        }
    }
}
