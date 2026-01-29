using CourseProject.Models;
using CourseProject.Services;

namespace CourseProject.Repositories;

/// <summary>
/// Repository Pattern для абстракції доступу до розподіленого сховища
/// </summary>
public interface IShipmentRepository
{
    Task SaveShipmentAsync(Shipment shipment);
    Task<Shipment?> GetShipmentAsync(string trackNumber);
    Task UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus status);
    Task<IEnumerable<Shipment>> GetShipmentsByRegionAsync(int regionId);
}

/// <summary>
/// Реалізація Repository з розподіленим зберіганням
/// Використовує IShardingStrategy для визначення цільового шарду
/// </summary>
public class DistributedShipmentRepository : IShipmentRepository
{
    private readonly IShardingStrategy _sharding;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DistributedShipmentRepository> _logger;
    
    // In-memory "shard storage" для демонстрації
    private static readonly Dictionary<int, List<Shipment>> _shardStorage = new();
    
    public DistributedShipmentRepository(
        IShardingStrategy sharding, 
        IHttpClientFactory httpClientFactory,
        ILogger<DistributedShipmentRepository> logger)
    {
        _sharding = sharding;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Ініціалізуємо 3 шарди
        for (int i = 0; i < 3; i++)
            _shardStorage[i] = new List<Shipment>();
    }

    public async Task SaveShipmentAsync(Shipment shipment)
    {
        int shardId = _sharding.GetShardId(shipment.TrackNumber);
        _shardStorage[shardId].Add(shipment);
        string msg = $"📦 Посилка {shipment.TrackNumber} збережена у ШАРД #{shardId} | Статус: {shipment.Status}";
        _logger.LogInformation(msg);
        Console.WriteLine(msg);
        await Task.CompletedTask;
    }

    public async Task<Shipment?> GetShipmentAsync(string trackNumber)
    {
        int shardId = _sharding.GetShardId(trackNumber);
        var shipment = _shardStorage[shardId].FirstOrDefault(s => s.TrackNumber == trackNumber);
        string msg = $"🔍 Пошук {trackNumber} у ШАРД #{shardId}: {(shipment != null ? "Знайдено" : "Не знайдено")}";
        _logger.LogInformation(msg);
        Console.WriteLine(msg);
        return await Task.FromResult(shipment);
    }

    public async Task UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus status)
    {
        // Шукаємо у всіх шардах (в реальній системі це було б складніше)
        foreach (var shard in _shardStorage.Values)
        {
            var shipment = shard.FirstOrDefault(s => s.Id == shipmentId);
            if (shipment != null)
            {
                shipment.Status = status;
                string msg = $"✓ Статус посилки {shipmentId} оновлено на {status}";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);
                break;
            }
        }
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Shipment>> GetShipmentsByRegionAsync(int regionId)
    {
        var regionShipments = _shardStorage.Values
            .SelectMany(s => s)
            .Where(s => s.RegionId == regionId)
            .ToList();
        string msg = $"📊 Знайдено {regionShipments.Count} посилок у регіоні #{regionId}";
        _logger.LogInformation(msg);
        Console.WriteLine(msg);
        return await Task.FromResult(regionShipments);
    }
}