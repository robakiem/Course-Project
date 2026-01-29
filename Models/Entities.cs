namespace CourseProject.Models;

/// <summary>
/// Статус посилки в системі
/// </summary>
public enum ShipmentStatus { Stored, InTransit, Delivered }

/// <summary>
/// Вузол (шард) розподіленої системи
/// </summary>
public class Node
{
    public int Id { get; set; }
    public string RegionName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Посилка з інформацією про статус та регіональність
/// </summary>
public class Shipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrackNumber { get; set; } = null!;
    public int RegionId { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedCourierId { get; set; }
}

/// <summary>
/// Кур'єр системи з координатами та статусом
/// </summary>
public class Courier
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public int RegionId { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public DateTime LastLocationUpdate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Телеметрія (GPS дані) кур'єра
/// </summary>
public record Telemetry(Guid CourierId, double Lat, double Lon, DateTime Timestamp);

/// <summary>
/// Статистика по регіону для моніторингу
/// </summary>
public class RegionStatistics
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = null!;
    public int TotalShipments { get; set; }
    public int DeliveredShipments { get; set; }
    public int ActiveCouriers { get; set; }
    public long TotalProcessedRequests { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}