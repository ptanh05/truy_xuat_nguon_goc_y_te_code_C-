using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class BatchOperationService : IBatchOperationService
    {
        private readonly PharmaDNAContext _context;
        private readonly INFTService _nftService;
        private readonly IInventoryService _inventoryService;

        public BatchOperationService(PharmaDNAContext context, INFTService nftService, IInventoryService inventoryService)
        {
            _context = context;
            _nftService = nftService;
            _inventoryService = inventoryService;
        }

        public async Task<BatchOperation> CreateBatchOperationAsync(string operationType, string fileName, int userId)
        {
            var batchOperation = new BatchOperation
            {
                OperationType = operationType,
                FileName = fileName,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId.ToString(),
                ProcessedRecords = 0,
                FailedRecords = 0,
                ProgressPercentage = 0
            };

            _context.BatchOperations.Add(batchOperation);
            await _context.SaveChangesAsync();
            return batchOperation;
        }

        public async Task<BatchOperation> GetBatchOperationAsync(int batchId)
        {
            return await _context.BatchOperations
                .Include(b => b.CreatedByUser)
                .Include(b => b.Details)
                .FirstOrDefaultAsync(b => b.Id == batchId);
        }

        public async Task<IEnumerable<BatchOperation>> GetAllBatchOperationsAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.BatchOperations
                .Include(b => b.CreatedByUser)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateBatchOperationStatusAsync(int batchId, string status)
        {
            var batch = await _context.BatchOperations.FindAsync(batchId);
            if (batch == null) return false;

            batch.Status = status;
            if (status == "Processing") batch.StartedAt = DateTime.UtcNow;
            if (status == "Completed" || status == "Failed") batch.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBatchProgressAsync(int batchId, int processedRecords, int failedRecords)
        {
            var batch = await _context.BatchOperations.FindAsync(batchId);
            if (batch == null) return false;

            batch.ProcessedRecords = processedRecords;
            batch.FailedRecords = failedRecords;
            batch.ProgressPercentage = batch.TotalRecords > 0 
                ? (int)((double)(processedRecords + failedRecords) / batch.TotalRecords * 100)
                : 0;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBatchDetailAsync(int batchId, int recordNumber, string data, bool success, string errorMessage = null)
        {
            var detail = new BatchOperationDetail
            {
                BatchOperationId = batchId,
                RecordNumber = recordNumber,
                Data = data,
                Status = success ? "Success" : "Failed",
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            };

            _context.BatchOperationDetails.Add(detail);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BatchOperationDetail>> GetBatchDetailsAsync(int batchId)
        {
            return await _context.BatchOperationDetails
                .Where(d => d.BatchOperationId == batchId)
                .OrderBy(d => d.RecordNumber)
                .ToListAsync();
        }

        public async Task<bool> ProcessCreateNFTBatchAsync(int batchId, List<CreateNFTBatchItem> items)
        {
            var batch = await GetBatchOperationAsync(batchId);
            if (batch == null) return false;

            batch.TotalRecords = items.Count;
            await UpdateBatchOperationStatusAsync(batchId, "Processing");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var (item, index) in items.Select((x, i) => (x, i)))
            {
                try
                {
                    var nft = new NFT
                    {
                        ProductName = item.ProductName,
                        Manufacturer = item.Manufacturer,
                        BatchNumber = item.BatchNumber,
                        ExpiryDate = item.ExpiryDate,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.NFTs.Add(nft);
                    await _context.SaveChangesAsync();

                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), true);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), false, ex.Message);
                }

                await UpdateBatchProgressAsync(batchId, processedCount, failedCount);
            }

            await UpdateBatchOperationStatusAsync(batchId, failedCount == 0 ? "Completed" : "Completed");
            return true;
        }

        public async Task<bool> ProcessUpdatePriceBatchAsync(int batchId, List<UpdatePriceBatchItem> items)
        {
            var batch = await GetBatchOperationAsync(batchId);
            if (batch == null) return false;

            batch.TotalRecords = items.Count;
            await UpdateBatchOperationStatusAsync(batchId, "Processing");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var (item, index) in items.Select((x, i) => (x, i)))
            {
                try
                {
                    var nft = await _context.NFTs.FindAsync(item.NFTId);
                    if (nft == null) throw new Exception("NFT not found");

                    nft.Price = item.NewPrice;
                    _context.NFTs.Update(nft);
                    await _context.SaveChangesAsync();

                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), true);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), false, ex.Message);
                }

                await UpdateBatchProgressAsync(batchId, processedCount, failedCount);
            }

            await UpdateBatchOperationStatusAsync(batchId, "Completed");
            return true;
        }

        public async Task<bool> ProcessImportInventoryBatchAsync(int batchId, List<ImportInventoryBatchItem> items)
        {
            var batch = await GetBatchOperationAsync(batchId);
            if (batch == null) return false;

            batch.TotalRecords = items.Count;
            await UpdateBatchOperationStatusAsync(batchId, "Processing");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var (item, index) in items.Select((x, i) => (x, i)))
            {
                try
                {
                    var nft = await _context.NFTs.FindAsync(item.NFTId);
                    if (nft == null) throw new Exception("NFT not found");

                    var location = new InventoryLocation
                    {
                        NFTId = item.NFTId,
                        LocationName = item.LocationName,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryLocations.Add(location);
                    await _context.SaveChangesAsync();

                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), true);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), false, ex.Message);
                }

                await UpdateBatchProgressAsync(batchId, processedCount, failedCount);
            }

            await UpdateBatchOperationStatusAsync(batchId, "Completed");
            return true;
        }

        public async Task<bool> ProcessTransferBatchAsync(int batchId, List<TransferBatchItem> items)
        {
            var batch = await GetBatchOperationAsync(batchId);
            if (batch == null) return false;

            batch.TotalRecords = items.Count;
            await UpdateBatchOperationStatusAsync(batchId, "Processing");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var (item, index) in items.Select((x, i) => (x, i)))
            {
                try
                {
                    var transfer = new TransferRequest
                    {
                        NFTId = item.NFTId,
                        FromUserId = item.FromUserId.ToString(),
                        ToUserId = item.ToUserId.ToString(),
                        Quantity = item.Quantity,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.TransferRequests.Add(transfer);
                    await _context.SaveChangesAsync();

                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), true);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    await AddBatchDetailAsync(batchId, index + 1, JsonSerializer.Serialize(item), false, ex.Message);
                }

                await UpdateBatchProgressAsync(batchId, processedCount, failedCount);
            }

            await UpdateBatchOperationStatusAsync(batchId, "Completed");
            return true;
        }
    }
}
