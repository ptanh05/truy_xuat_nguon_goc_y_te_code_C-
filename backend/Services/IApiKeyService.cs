using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IApiKeyService
    {
        Task<ApiKey> CreateApiKeyAsync(int userId, string name, string description, int rateLimit = 1000);
        Task<ApiKey> GetApiKeyAsync(int keyId);
        Task<IEnumerable<ApiKey>> GetUserApiKeysAsync(int userId);
        Task<bool> ValidateApiKeyAsync(string key, string secret);
        Task<bool> RevokeApiKeyAsync(int keyId);
        Task<bool> LogApiUsageAsync(int apiKeyId, string endpoint, string method, int statusCode, long responseTimeMs, string ipAddress);
        Task<IEnumerable<ApiKeyUsage>> GetApiKeyUsageAsync(int apiKeyId, int days = 30);
        Task<Dictionary<string, object>> GetApiStatisticsAsync(int apiKeyId);
    }
}
