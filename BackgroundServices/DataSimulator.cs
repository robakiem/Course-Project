using CourseProject.Models;

namespace CourseProject.BackgroundServices;

public class DataSimulator : BackgroundService
{
    private readonly IHttpClientFactory _clientFactory;

    public DataSimulator(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = _clientFactory.CreateClient();
        while (!stoppingToken.IsCancellationRequested)
        {
            var shipment = new Shipment { TrackNumber = "UA-" + Guid.NewGuid().ToString().Substring(0, 8) };
            
            try {
                await client.PostAsJsonAsync("https://localhost:7001/api/gateway/register", shipment);
            } catch { /* ігноруємо помилки при старті */ }

            await Task.Delay(3000, stoppingToken); // Кожні 3 секунди нова посилка
        }
    }
}