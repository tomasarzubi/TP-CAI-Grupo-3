using Notifications.API.Endpoints;
using Notifications.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => 
config.ReadFrom.Configuration(context.Configuration));

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar HttpClient para consumir la API de Usuarios 
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5102"); // Puerto donde corre Users.API
});

// Registrar el servicio de notificaciones
builder.Services.AddScoped<NotificationService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));


builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();
app.UseExceptionHandler();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapear los endpoints definidos en NotificationEndpoints.cs
app.MapNotificationEndpoints();

app.Run();