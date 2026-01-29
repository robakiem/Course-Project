using Microsoft.AspNetCore.Mvc;
using CourseProject.Models;
using CourseProject.Repositories;
using CourseProject.Services;

namespace CourseProject.Controllers;

/// <summary>
/// Edge Gateway API - центральна точка входу для розподіленої системи
/// Реалізує логіку маршрутизації запитів до відповідних шардів
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IShipmentRepository _repository;
    private readonly IShardingStrategy _sharding;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(
        IShipmentRepository repository,
        IShardingStrategy sharding,
        ILogger<GatewayController> logger)
    {
        _repository = repository;
        _sharding = sharding;
        _logger = logger;
    }

    /// <summary>
    /// Реєстрація нової посилки в системі
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Shipment shipment)
    {
        if (string.IsNullOrEmpty(shipment.TrackNumber))
            return BadRequest(new { error = "TrackNumber не може бути пустим" });

        await _repository.SaveShipmentAsync(shipment);
        
        return CreatedAtAction(nameof(GetShipment), 
            new { trackNumber = shipment.TrackNumber },
            new { 
                message = "Посилка успішно зареєстрована", 
                trackNumber = shipment.TrackNumber,
                id = shipment.Id
            });
    }

    /// <summary>
    /// Отримання інформації про посилку за номером трекування
    /// </summary>
    [HttpGet("shipment/{trackNumber}")]
    public async Task<IActionResult> GetShipment(string trackNumber)
    {
        var shipment = await _repository.GetShipmentAsync(trackNumber);
        
        if (shipment == null)
            return NotFound(new { error = $"Посилка {trackNumber} не знайдена" });

        return Ok(shipment);
    }

    /// <summary>
    /// Оновлення статусу посилки
    /// </summary>
    [HttpPut("shipment/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        await _repository.UpdateShipmentStatusAsync(id, request.Status);
        return Ok(new { message = $"Статус посилки оновлено на {request.Status}" });
    }

    /// <summary>
    /// Отримання всіх посилок за регіоном
    /// </summary>
    [HttpGet("region/{regionId}")]
    public async Task<IActionResult> GetShipmentsByRegion(int regionId)
    {
        var shipments = await _repository.GetShipmentsByRegionAsync(regionId);
        return Ok(new {
            regionId,
            count = shipments.Count(),
            shipments
        });
    }

    /// <summary>
    /// Інформація про стратегію шардингу (для моніторингу)
    /// </summary>
    [HttpGet("sharding-info")]
    public IActionResult GetShardingInfo()
    {
        return Ok(new {
            strategy = _sharding.GetStrategyName(),
            description = "Система використовує розподілене зберігання для масштабованості"
        });
    }
}

/// <summary>
/// DTO для оновлення статусу
/// </summary>
public class UpdateStatusRequest
{
    public ShipmentStatus Status { get; set; }
}