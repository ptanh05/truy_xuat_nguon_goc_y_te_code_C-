using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace PharmaDNA.Web.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    if (value is T directValue)
                    {
                        return directValue;
                    }

                    if (value is string jsonString)
                    {
                        return JsonSerializer.Deserialize<T>(jsonString);
                    }
                }

                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cache key: {key}");
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expiration ?? TimeSpan.FromMinutes(30)
                };

                _cache.Set(key, value, options);
                _logger.LogDebug($"Cache set for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cache key: {key}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _cache.Remove(key);
                _logger.LogDebug($"Cache removed for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache key: {key}");
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // MemoryCache doesn't support pattern removal natively
            // This is a simplified implementation
            _logger.LogWarning("Pattern-based cache removal not supported in MemoryCache");
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
    }
}
