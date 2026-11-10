/*
using Orders.API.Data;
using Orders.API.HealthChecks;

namespace Orders.API.Extensions
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

using Dapper;
using Orders.API.Data;
using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Services;

namespace Orders.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new UtcDateTimeTypeHandler());

            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<OrderRepository>();
            services.AddScoped<OrderService>();

            // Clientes HTTP hacia Products.API y Users.API
            services.AddHttpClient<ProductsClient>(client =>
            client.BaseAddress = new Uri("http://localhost:5112"));

            services.AddHttpClient<UsersClient>(client =>
            client.BaseAddress = new Uri("http://localhost:5102"));      

            
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
                setup.AddHealthCheckEndpoint("Orders.API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
