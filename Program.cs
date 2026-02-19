using CourseProject.Services;
using CourseProject.Repositories;
using CourseProject.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Явно налаштовуємо логування в консоль для Information і вище
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ✅ Додаємо MVC + API
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IShardingStrategy>(new HashShardingStrategy(3));
builder.Services.AddScoped<IShipmentRepository, DistributedShipmentRepository>();
builder.Services.AddHostedService<DataSimulator>();

var app = builder.Build();

// ✅ Статичні файли (CSS, JS, картинки)
app.UseStaticFiles();

// Налаштування Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Post API V1");
    });
}

// ✅ MVC маршрутизація
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();