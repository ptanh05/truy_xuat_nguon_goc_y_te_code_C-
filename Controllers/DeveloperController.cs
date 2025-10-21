using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeveloperController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public DeveloperController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost("api-keys")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            var apiKey = await _apiKeyService.CreateApiKeyAsync(
                request.UserId,
                request.Name,
                request.Description,
                request.RateLimit ?? 1000
            );

            return Ok(new
            {
                message = "API key created successfully",
                key = apiKey.Key,
                secret = "sk_" + Guid.NewGuid().ToString().Replace("-", ""), // Return secret only once
                keyId = apiKey.Id
            });
        }

        [HttpGet("api-keys")]
        public async Task<IActionResult> GetApiKeys(int userId)
        {
            var keys = await _apiKeyService.GetUserApiKeysAsync(userId);
            return Ok(keys);
        }

        [HttpDelete("api-keys/{keyId}")]
        public async Task<IActionResult> RevokeApiKey(int keyId)
        {
            var result = await _apiKeyService.RevokeApiKeyAsync(keyId);
            if (!result) return NotFound();
            return Ok(new { message = "API key revoked successfully" });
        }

        [HttpGet("api-keys/{keyId}/usage")]
        public async Task<IActionResult> GetApiKeyUsage(int keyId, int days = 30)
        {
            var usage = await _apiKeyService.GetApiKeyUsageAsync(keyId, days);
            return Ok(usage);
        }

        [HttpGet("api-keys/{keyId}/statistics")]
        public async Task<IActionResult> GetApiKeyStatistics(int keyId)
        {
            var stats = await _apiKeyService.GetApiStatisticsAsync(keyId);
            return Ok(stats);
        }
    }
}
