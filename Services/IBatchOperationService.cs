using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IBatchOperationService
    {
        // Batch Operation Management
        Task<BatchOperation> CreateBatchOperationAsync(string operationType, string fileName, int userId);
        Task<BatchOperation> GetBatchOperationAsync(int batchId);
        Task<IEnumerable<BatchOperation>> GetAllBatchOperationsAsync(int pageNumber = 1, int pageSize = 10);
        Task<bool> UpdateBatchOperationStatusAsync(int batchId, string status);
        Task<bool> UpdateBatchProgressAsync(int batchId, int processedRecords, int failedRecords);

        // Batch Details
        Task<bool> AddBatchDetailAsync(int batchId, int recordNumber, string data, bool success, string errorMessage = null);
        Task<IEnumerable<BatchOperationDetail>> GetBatchDetailsAsync(int batchId);

        // Batch Operations
        Task<bool> ProcessCreateNFTBatchAsync(int batchId, List<CreateNFTBatchItem> items);
        Task<bool> ProcessUpdatePriceBatchAsync(int batchId, List<UpdatePriceBatchItem> items);
        Task<bool> ProcessImportInventoryBatchAsync(int batchId, List<ImportInventoryBatchItem> items);
        Task<bool> ProcessTransferBatchAsync(int batchId, List<TransferBatchItem> items);
    }
}
