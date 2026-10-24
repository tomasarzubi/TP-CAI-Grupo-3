/*using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Cart.API.Middleware;
using Serilog;

namespace Cart.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex != null) ? Serilog.Events.LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;
            });

            app.UseMiddleware<AuditMiddleware>();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}
*/

//------------CAMBIOS---------------

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Cart.API.Exceptions;
using Cart.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Cart.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // 1. Excepciones: primero, para capturar lo que falle más abajo
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 2. CorrelationId
            app.UseMiddleware<CorrelationIdMiddleware>();

            // 3. Logging de requests (inicio/fin con duración)
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex is NotFoundException or BusinessRuleException or ValidationException) ? LogEventLevel.Warning :
                    (ex != null) ? LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? LogEventLevel.Verbose : LogEventLevel.Information;
            });

            // 4. Auditoría
            app.UseMiddleware<AuditMiddleware>();

            // 5. Health checks
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("database"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}
