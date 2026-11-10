using Notifications.API.Endpoints;
using Notifications.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Logging (Serilog va primero)
builder.AddAppLogging();

// 2. Servicios
builder.Services.AddAppServices();
var app = builder.Build();

// 3. Middleware
app.UseAppMiddleware();

// 4. Endpoints
app.MapNotificationEndpoints();

app.Run();