using Hangfire.MemoryStorage;
//using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire
{
    public static class HangfireConfig
    {
        public static void HangfireServiceConfig(this IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
                            .UseRecommendedSerializerSettings()
                            .UseMemoryStorage());
                            //.UseSQLiteStorage());

            // Define a quantidade de retentativas aplicadas a um job com falha.
            // Por padrão serão 10, aqui estamos definindo 3 tentativas
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = 3,
                DelaysInSeconds = new int[] { 300 }
            });

            services.AddHangfireServer();
        }

        public static void HangfireApplicationConfig(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard("/hangfire");
        }
    }
}