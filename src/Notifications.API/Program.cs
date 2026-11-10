using Notifications.API.Endpoints;
using Notifications.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar HttpClient para consumir la API de Usuarios (Users.API)
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5112"); // Puerto donde corre Users.API
});

// Registrar el servicio de notificaciones
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapear los endpoints definidos en NotificationEndpoints.cs
app.MapNotificationEndpoints();

app.Run();