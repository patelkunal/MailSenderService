using Amazon;
using Amazon.SimpleEmail.Model;
using EmailCoreInfra;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AwsSimpleEmailServiceInfra.Tests
{
    [TestClass]
    public class SesEmailTemplateOperationsImplTests
    {
        private static readonly RegionEndpoint _regionEndpoint = RegionEndpoint.USEast2;
        private static ILoggerFactory loggerFactory;
        private static ILogger<SesEmailTemplateOperationsImpl> _logger;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<SesEmailTemplateOperationsImpl>();
        }


        [TestMethod]
        public void TestCreateOrUpdateTemplate()
        {
            string templateName = "TestTemplate001";
            IEmailTemplateOperations templateOperations = new SesEmailTemplateOperationsImpl(_regionEndpoint, _logger);
            try
            {
                templateOperations.CreateOrUpdateEmailTemplate(
                    templateName,
                    "This is Subject for TestTemplate001",
                    "This is text-content for TestTemplate001",
                    "This is html-content for TestTemplate001"
                );
                templateOperations.GetEmailTemplate(templateName);
                templateOperations.CreateOrUpdateEmailTemplate(
                    templateName,
                    "This is Subject for TestTemplate001 - updated",
                    "This is text-content for TestTemplate001 - updated",
                    "This is html-content for TestTemplate001 - updated"
                );
                templateOperations.GetEmailTemplate(templateName);
            }
            catch
            {
                Assert.Fail("Not expecting exception while CreateOrUpdateTemplate");
            }
            finally
            {
                templateOperations.DeleteEmailTemplate(templateName);
            }
        }

        [TestMethod]
        public void TestDeleteTemplate()
        {
            string templateName = "TestTemplate001";
            IEmailTemplateOperations templateOperations = new SesEmailTemplateOperationsImpl(_regionEndpoint, _logger);

            templateOperations.CreateOrUpdateEmailTemplate(
                templateName,
                "This is Subject for TestTemplate001",
                "This is text-content for TestTemplate001",
                "This is html-content for TestTemplate001"
            );
            templateOperations.GetEmailTemplate(templateName);

            templateOperations.DeleteEmailTemplate(templateName);
            Assert.ThrowsException<TemplateDoesNotExistException>(() => templateOperations.GetEmailTemplate(templateName));
        }

        [TestMethod()]
        public void GetEmailTemplateTest()
        {
            string nonExistantTemplateName = "NonExistentTestTemplate001";
            IEmailTemplateOperations templateOperations = new SesEmailTemplateOperationsImpl(_regionEndpoint, _logger);
            Assert.ThrowsException<TemplateDoesNotExistException>(() => templateOperations.GetEmailTemplate(nonExistantTemplateName));
        }
    }
}
