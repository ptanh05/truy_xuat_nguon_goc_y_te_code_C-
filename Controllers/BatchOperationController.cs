using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchOperationController : ControllerBase
    {
        private readonly IBatchOperationService _batchOperationService;

        public BatchOperationController(IBatchOperationService batchOperationService)
        {
            _batchOperationService = batchOperationService;
        }

        [HttpPost("create-nft-batch")]
        public async Task<IActionResult> CreateNFTBatch(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            try
            {
                var batch = await _batchOperationService.CreateBatchOperationAsync("CreateNFT", file.FileName, userId);

                var items = new List<CreateNFTBatchItem>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                {
                    items = csv.GetRecords<CreateNFTBatchItem>().ToList();
                }

                await _batchOperationService.ProcessCreateNFTBatchAsync(batch.Id, items);

                return Ok(new { message = "Batch operation started", batchId = batch.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("update-price-batch")]
        public async Task<IActionResult> UpdatePriceBatch(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            try
            {
                var batch = await _batchOperationService.CreateBatchOperationAsync("UpdatePrice", file.FileName, userId);

                var items = new List<UpdatePriceBatchItem>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                {
                    items = csv.GetRecords<UpdatePriceBatchItem>().ToList();
                }

                await _batchOperationService.ProcessUpdatePriceBatchAsync(batch.Id, items);

                return Ok(new { message = "Batch operation started", batchId = batch.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{batchId}")]
        public async Task<IActionResult> GetBatchOperation(int batchId)
        {
            var batch = await _batchOperationService.GetBatchOperationAsync(batchId);
            if (batch == null) return NotFound();
            return Ok(batch);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBatchOperations(int pageNumber = 1, int pageSize = 10)
        {
            var batches = await _batchOperationService.GetAllBatchOperationsAsync(pageNumber, pageSize);
            return Ok(batches);
        }

        [HttpGet("{batchId}/details")]
        public async Task<IActionResult> GetBatchDetails(int batchId)
        {
            var details = await _batchOperationService.GetBatchDetailsAsync(batchId);
            return Ok(details);
        }
    }
}
