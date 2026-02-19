using Microsoft.AspNetCore.Mvc;
using CourseProject.Models;
using CourseProject.Repositories;

namespace CourseProject.Controllers;

/// <summary>
/// HomeController - обслуговує фронтенд сторінки
/// </summary>
public class HomeController : Controller
{
    private readonly IShipmentRepository _repository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IShipmentRepository repository, ILogger<HomeController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Головна сторінка з динамічною картою
    /// </summary>
    public IActionResult Index()
    {
        ViewData["Title"] = "Smart-Post - Моніторинг доставок";
        return View();
    }

    /// <summary>
    /// Сторінка трекингу посилок
    /// </summary>
    public IActionResult TrackShipment()
    {
        ViewData["Title"] = "Трекер посилок";
        return View();
    }

    /// <summary>
    /// Адміністративна панель
    /// </summary>
    public IActionResult Dashboard()
    {
        ViewData["Title"] = "Панель керування";
        return View();
    }

    /// <summary>
    /// API для AJAX - отримати усі посилки за регіоном (для карти)
    /// </summary>
    [HttpGet("api/shipments/region/{regionId}")]
    public async Task<IActionResult> GetShipmentsForMap(int regionId)
    {
        var shipments = await _repository.GetShipmentsByRegionAsync(regionId);
        return Json(shipments);
    }
}
