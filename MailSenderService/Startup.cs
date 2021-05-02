namespace MailSenderService
{
    using Amazon;
    using AwsSimpleEmailServiceInfra;
    using EmailCoreInfra;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using ScheduledEmailTriggerInfra;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IScheduledEmailTriggerWorker, ScheduledEmailTriggerWorker>();
            services.AddSingleton<IScheduledEmailTriggerService, ScheduledEmailTriggerServiceImpl>();
            services.AddSingleton<IMessageDatabaseOperations, NoOpMessageDatabaseOperationsImpl>();
            services.AddSingleton<IEmailOperations, SesEmailOperationsImpl>(provider =>
                new SesEmailOperationsImpl(
                        Configuration.GetSection("EmailInfra")["FromEmailAddress"],
                        Configuration.GetSection("EmailInfra")["DefaultConfigurationSet"],
                        RegionEndpoint.GetBySystemName(Configuration.GetSection("EmailInfra")["AwsSesRegion"]),
                        provider.GetRequiredService<ILogger<SesEmailOperationsImpl>>()
                    )
            );
            services.AddSingleton<IEmailTemplateOperations, SesEmailTemplateOperationsImpl>(provider =>
                new SesEmailTemplateOperationsImpl(
                        RegionEndpoint.GetBySystemName(Configuration.GetSection("EmailInfra")["AwsSesRegion"]),
                        provider.GetRequiredService<ILogger<SesEmailTemplateOperationsImpl>>()
                    )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
