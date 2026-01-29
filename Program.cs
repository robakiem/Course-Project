using CourseProject.Services;
using CourseProject.Repositories;
using CourseProject.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Явно налаштовуємо логування в консоль для Information і вище
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Додаємо тільки базові сервіси
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Використовує Swashbuckle

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IShardingStrategy>(new HashShardingStrategy(3));
builder.Services.AddScoped<IShipmentRepository, DistributedShipmentRepository>();
builder.Services.AddHostedService<DataSimulator>();

var app = builder.Build();

// Налаштування Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Post API V1");
    });
}

app.MapControllers();
app.Run();