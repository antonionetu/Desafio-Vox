using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Vox.Infrastructure.Utils
{
    public class CacheManager
    {
        private readonly IDistributedCache _cache;
        private static readonly string KeyRegistryPrefix = "CacheKeys:";

        public CacheManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
            };

            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);

            var prefix = GetPrefixFromKey(key);
            if (!string.IsNullOrEmpty(prefix))
            {
                var listKey = KeyRegistryPrefix + prefix;
                var existing = await _cache.GetStringAsync(listKey);
                var keys = string.IsNullOrEmpty(existing)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(existing)!;

                if (!keys.Contains(key))
                    keys.Add(key);

                await _cache.SetStringAsync(listKey, JsonSerializer.Serialize(keys), options);
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            var listKey = KeyRegistryPrefix + prefix;
            var existing = await _cache.GetStringAsync(listKey);

            if (string.IsNullOrEmpty(existing)) return;

            var keys = JsonSerializer.Deserialize<List<string>>(existing)!;

            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key);
            }

            await _cache.RemoveAsync(listKey);
        }

        private string GetPrefixFromKey(string key)
        {
            var parts = key.Split(':');
            return parts.Length > 1 ? parts[0] : string.Empty;
        }
    }
}
