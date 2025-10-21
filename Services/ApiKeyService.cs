using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly PharmaDNAContext _context;

        public ApiKeyService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<ApiKey> CreateApiKeyAsync(int userId, string name, string description, int rateLimit = 1000)
        {
            var key = GenerateApiKey();
            var secret = GenerateSecret();

            var apiKey = new ApiKey
            {
                UserId = userId,
                Key = key,
                Secret = HashSecret(secret),
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RateLimit = rateLimit
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return apiKey;
        }

        public async Task<ApiKey> GetApiKeyAsync(int keyId)
        {
            return await _context.ApiKeys
                .Include(k => k.User)
                .FirstOrDefaultAsync(k => k.Id == keyId);
        }

        public async Task<IEnumerable<ApiKey>> GetUserApiKeysAsync(int userId)
        {
            return await _context.ApiKeys
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ValidateApiKeyAsync(string key, string secret)
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == key && k.IsActive);

            if (apiKey == null) return false;

            var isValid = BCrypt.Net.BCrypt.Verify(secret, apiKey.Secret);
            if (isValid)
            {
                apiKey.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return isValid;
        }

        public async Task<bool> RevokeApiKeyAsync(int keyId)
        {
            var apiKey = await _context.ApiKeys.FindAsync(keyId);
            if (apiKey == null) return false;

            apiKey.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogApiUsageAsync(int apiKeyId, string endpoint, string method, int statusCode, long responseTimeMs, string ipAddress)
        {
            var usage = new ApiKeyUsage
            {
                ApiKeyId = apiKeyId,
                Endpoint = endpoint,
                Method = method,
                StatusCode = statusCode,
                ResponseTimeMs = responseTimeMs,
                IpAddress = ipAddress,
                UsedAt = DateTime.UtcNow
            };

            _context.ApiKeyUsages.Add(usage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ApiKeyUsage>> GetApiKeyUsageAsync(int apiKeyId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.ApiKeyUsages
                .Where(u => u.ApiKeyId == apiKeyId && u.UsedAt >= startDate)
                .OrderByDescending(u => u.UsedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetApiStatisticsAsync(int apiKeyId)
        {
            var usages = await GetApiKeyUsageAsync(apiKeyId, 30);
            var usageList = usages.ToList();

            return new Dictionary<string, object>
            {
                { "TotalRequests", usageList.Count },
                { "AverageResponseTime", usageList.Count > 0 ? usageList.Average(u => u.ResponseTimeMs) : 0 },
                { "SuccessRate", usageList.Count > 0 ? (double)usageList.Count(u => u.StatusCode < 400) / usageList.Count * 100 : 0 },
                { "TopEndpoints", usageList.GroupBy(u => u.Endpoint).OrderByDescending(g => g.Count()).Take(5).Select(g => new { Endpoint = g.Key, Count = g.Count() }) },
                { "ErrorCount", usageList.Count(u => u.StatusCode >= 400) }
            };
        }

        private string GenerateApiKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return "pk_" + new string(Enumerable.Range(0, 32).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private string GenerateSecret()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        private string HashSecret(string secret)
        {
            return BCrypt.Net.BCrypt.HashPassword(secret);
        }
    }
}
