using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly PharmaDNAContext _context;

        public InventoryService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<InventoryLocation> CreateLocationAsync(InventoryLocation location)
        {
            _context.InventoryLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<InventoryLocation> GetLocationAsync(int id)
        {
            return await _context.InventoryLocations
                .Include(l => l.InventoryItems)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<InventoryLocation>> GetAllLocationsAsync()
        {
            return await _context.InventoryLocations
                .Where(l => l.IsActive)
                .ToListAsync();
        }

        public async Task UpdateLocationAsync(InventoryLocation location)
        {
            location.UpdatedDate = DateTime.UtcNow;
            _context.InventoryLocations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(int id)
        {
            var location = await _context.InventoryLocations.FindAsync(id);
            if (location != null)
            {
                location.IsActive = false;
                await UpdateLocationAsync(location);
            }
        }

        public async Task<InventoryItem> AddInventoryItemAsync(InventoryItem item)
        {
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            await CheckAndCreateAlertsAsync();
            return item;
        }

        public async Task<InventoryItem> GetInventoryItemAsync(int id)
        {
            return await _context.InventoryItems
                .Include(i => i.NFT)
                .Include(i => i.Location)
                .Include(i => i.Movements)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<InventoryItem>> GetInventoryByLocationAsync(int locationId)
        {
            return await _context.InventoryItems
                .Where(i => i.LocationId == locationId)
                .Include(i => i.NFT)
                .ToListAsync();
        }

        public async Task<List<InventoryItem>> GetInventoryByNFTAsync(int nftId)
        {
            return await _context.InventoryItems
                .Where(i => i.NFTId == nftId)
                .Include(i => i.Location)
                .ToListAsync();
        }

        public async Task UpdateInventoryItemAsync(InventoryItem item)
        {
            item.UpdatedDate = DateTime.UtcNow;
            _context.InventoryItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<InventoryMovement> RecordMovementAsync(InventoryMovement movement)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(movement.InventoryItemId);
            if (inventoryItem != null)
            {
                if (movement.MovementType == "In")
                    inventoryItem.Quantity += movement.Quantity;
                else if (movement.MovementType == "Out" || movement.MovementType == "Transfer")
                    inventoryItem.Quantity -= movement.Quantity;

                await UpdateInventoryItemAsync(inventoryItem);
            }

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
            await CheckAndCreateAlertsAsync();
            return movement;
        }

        public async Task<List<InventoryMovement>> GetMovementHistoryAsync(int inventoryItemId)
        {
            return await _context.InventoryMovements
                .Where(m => m.InventoryItemId == inventoryItemId)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<List<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryMovements
                .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate)
                .Include(m => m.InventoryItem)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<List<InventoryAlert>> GetActiveAlertsAsync()
        {
            return await _context.InventoryAlerts
                .Where(a => !a.IsResolved)
                .Include(a => a.InventoryItem)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<InventoryAlert>> GetAlertsByLocationAsync(int locationId)
        {
            return await _context.InventoryAlerts
                .Where(a => !a.IsResolved && a.InventoryItem.LocationId == locationId)
                .Include(a => a.InventoryItem)
                .ToListAsync();
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            var items = await _context.InventoryItems.ToListAsync();

            foreach (var item in items)
            {
                if (item.Quantity <= item.ReorderLevel)
                {
                    var existingAlert = await _context.InventoryAlerts
                        .FirstOrDefaultAsync(a => a.InventoryItemId == item.Id && 
                                                   a.AlertType == "LowStock" && 
                                                   !a.IsResolved);

                    if (existingAlert == null)
                    {
                        var alert = new InventoryAlert
                        {
                            InventoryItemId = item.Id,
                            AlertType = "LowStock",
                            Severity = item.Quantity == 0 ? "High" : "Medium",
                            Message = $"Stock level for {item.Batch} is below reorder level ({item.Quantity}/{item.ReorderLevel})"
                        };
                        _context.InventoryAlerts.Add(alert);
                    }
                }

                var daysUntilExpiry = (item.ExpiryDate - DateTime.UtcNow).TotalDays;
                if (daysUntilExpiry <= 30 && daysUntilExpiry > 0)
                {
                    var existingAlert = await _context.InventoryAlerts
                        .FirstOrDefaultAsync(a => a.InventoryItemId == item.Id && 
                                                   a.AlertType == "ExpiryWarning" && 
                                                   !a.IsResolved);

                    if (existingAlert == null)
                    {
                        var alert = new InventoryAlert
                        {
                            InventoryItemId = item.Id,
                            AlertType = "ExpiryWarning",
                            Severity = daysUntilExpiry <= 7 ? "High" : "Medium",
                            Message = $"Product {item.Batch} expires in {(int)daysUntilExpiry} days"
                        };
                        _context.InventoryAlerts.Add(alert);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResolveAlertAsync(int alertId)
        {
            var alert = await _context.InventoryAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedDate = DateTime.UtcNow;
                _context.InventoryAlerts.Update(alert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, object>> GetInventorySummaryAsync()
        {
            var totalItems = await _context.InventoryItems.CountAsync();
            var totalQuantity = await _context.InventoryItems.SumAsync(i => i.Quantity);
            var lowStockCount = await _context.InventoryItems.CountAsync(i => i.Quantity <= i.ReorderLevel);
            var expiringCount = await _context.InventoryItems
                .CountAsync(i => (i.ExpiryDate - DateTime.UtcNow).TotalDays <= 30);

            return new Dictionary<string, object>
            {
                { "TotalItems", totalItems },
                { "TotalQuantity", totalQuantity },
                { "LowStockCount", lowStockCount },
                { "ExpiringCount", expiringCount },
                { "ActiveAlerts", await _context.InventoryAlerts.CountAsync(a => !a.IsResolved) }
            };
        }

        public async Task<List<InventoryItem>> GetLowStockItemsAsync()
        {
            return await _context.InventoryItems
                .Where(i => i.Quantity <= i.ReorderLevel)
                .Include(i => i.NFT)
                .Include(i => i.Location)
                .ToListAsync();
        }

        public async Task<List<InventoryItem>> GetExpiringItemsAsync(int daysUntilExpiry = 30)
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
            return await _context.InventoryItems
                .Where(i => i.ExpiryDate <= expiryDate && i.ExpiryDate > DateTime.UtcNow)
                .Include(i => i.NFT)
                .Include(i => i.Location)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetInventoryByLocationAsync()
        {
            var result = new Dictionary<string, int>();
            var locations = await _context.InventoryLocations.ToListAsync();

            foreach (var location in locations)
            {
                var quantity = await _context.InventoryItems
                    .Where(i => i.LocationId == location.Id)
                    .SumAsync(i => i.Quantity);
                result[location.LocationName] = quantity;
            }

            return result;
        }
    }
}
