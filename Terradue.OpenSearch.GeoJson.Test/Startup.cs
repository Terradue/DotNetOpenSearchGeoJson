using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Terradue.OpenSearch.GeoJson.Test
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            Configuration = GetApplicationConfiguration();
            services.AddLogging(builder =>
                {
                    builder.AddConfiguration(Configuration.GetSection("Logging"));
                });
            services.AddOptions();
        }

        public void Configure(ILoggerFactory loggerfactory)
        {
            // loggerfactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
            loggerfactory.AddLog4Net();
        }

        public IConfiguration GetApplicationConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return builder;
        }
    }
}