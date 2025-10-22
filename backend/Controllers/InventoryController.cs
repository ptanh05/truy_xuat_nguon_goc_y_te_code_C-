using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("locations")]
        public async Task<IActionResult> CreateLocation([FromBody] InventoryLocation location)
        {
            var result = await _inventoryService.CreateLocationAsync(location);
            return Ok(result);
        }

        [HttpGet("locations/{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            var location = await _inventoryService.GetLocationAsync(id);
            return Ok(location);
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _inventoryService.GetAllLocationsAsync();
            return Ok(locations);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItem item)
        {
            var result = await _inventoryService.AddInventoryItemAsync(item);
            return Ok(result);
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetInventoryItem(int id)
        {
            var item = await _inventoryService.GetInventoryItemAsync(id);
            return Ok(item);
        }

        [HttpGet("location/{locationId}")]
        public async Task<IActionResult> GetInventoryByLocation(int locationId)
        {
            var items = await _inventoryService.GetInventoryByLocationAsync(locationId);
            return Ok(items);
        }

        [HttpPost("movements")]
        public async Task<IActionResult> RecordMovement([FromBody] InventoryMovement movement)
        {
            var result = await _inventoryService.RecordMovementAsync(movement);
            return Ok(result);
        }

        [HttpGet("movements/{inventoryItemId}")]
        public async Task<IActionResult> GetMovementHistory(int inventoryItemId)
        {
            var movements = await _inventoryService.GetMovementHistoryAsync(inventoryItemId);
            return Ok(movements);
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetActiveAlerts()
        {
            var alerts = await _inventoryService.GetActiveAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockItems()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            return Ok(items);
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringItems([FromQuery] int days = 30)
        {
            var items = await _inventoryService.GetExpiringItemsAsync(days);
            return Ok(items);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            var summary = await _inventoryService.GetInventorySummaryAsync();
            return Ok(summary);
        }
    }
}
