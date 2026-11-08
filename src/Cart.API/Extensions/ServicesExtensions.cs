/*
using Cart.API.Data;
using Cart.API.HealthChecks;

namespace Cart.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<ItemRepository>();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();
        }
    }
}
*/
// ------------BASE EDITADA--------------------------------

using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Services;
using Cart.API.Clients;
using Dapper;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Cart.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new UtcDateTimeTypeHandler());

            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<CartRepository>();
            services.AddScoped<CartServices>();
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdHandler>();
            services.AddHttpClient<ProductsClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(config["Services:ProductsApi:BaseUrl"] ?? "http://localhost:5112");
                client.Timeout = TimeSpan.FromSeconds(5);
            }).AddHttpMessageHandler<CorrelationIdHandler>();

            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("Cart.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
