using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IInventoryService
    {
        // Location Management
        Task<InventoryLocation> CreateLocationAsync(InventoryLocation location);
        Task<InventoryLocation> GetLocationAsync(int id);
        Task<List<InventoryLocation>> GetAllLocationsAsync();
        Task UpdateLocationAsync(InventoryLocation location);
        Task DeleteLocationAsync(int id);

        // Inventory Items
        Task<InventoryItem> AddInventoryItemAsync(InventoryItem item);
        Task<InventoryItem> GetInventoryItemAsync(int id);
        Task<List<InventoryItem>> GetInventoryByLocationAsync(int locationId);
        Task<List<InventoryItem>> GetInventoryByNFTAsync(int nftId);
        Task UpdateInventoryItemAsync(InventoryItem item);

        // Inventory Movements
        Task<InventoryMovement> RecordMovementAsync(InventoryMovement movement);
        Task<List<InventoryMovement>> GetMovementHistoryAsync(int inventoryItemId);
        Task<List<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Alerts
        Task<List<InventoryAlert>> GetActiveAlertsAsync();
        Task<List<InventoryAlert>> GetAlertsByLocationAsync(int locationId);
        Task CheckAndCreateAlertsAsync();
        Task ResolveAlertAsync(int alertId);

        // Reports
        Task<Dictionary<string, object>> GetInventorySummaryAsync();
        Task<List<InventoryItem>> GetLowStockItemsAsync();
        Task<List<InventoryItem>> GetExpiringItemsAsync(int daysUntilExpiry = 30);
        Task<Dictionary<string, int>> GetInventoryByLocationAsync();
    }
}
