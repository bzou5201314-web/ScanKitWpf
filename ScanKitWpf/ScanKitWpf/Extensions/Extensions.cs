using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ScanKitWpf.ViewModels;

namespace ScanKitWpf.Extensions
{
    public static class Extensions
    {
        public static void AddNLogServices(this IServiceCollection services, IConfiguration configuration)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddNLog(configuration);
            });
            var logger = loggerFactory.CreateLogger(typeof(MainViewModel));
            services.AddSingleton(logger);
        }
    }
}
