using Notifications.API.ExceptionHandlers;
using Notifications.API.HealthChecks;
using Notifications.API.Services;

namespace Notifications.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<NotificationService>();
            services.AddHttpClient<UsersClient>(client => client.BaseAddress = new Uri("http://localhost:5102"));

            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Notifications.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}