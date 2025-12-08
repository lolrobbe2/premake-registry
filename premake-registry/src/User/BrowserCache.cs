#nullable enable

using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;
namespace premake.User
{
    namespace premake.Repo
    {
        public class BrowserCache<T>
        {
            private readonly IJSRuntime _js;

            public BrowserCache(IJSRuntime js)
            {
                _js = js;
            }

            private string GetCacheKey(object key)
            {
                var className = typeof(T).Name;
                return $"_{className}_{key}";
            }

            public async Task<T> CacheSetAsync(T item, object key, TimeSpan? ttl = null)
            {
                var cacheKey = GetCacheKey(key);

                var payload = new
                {
                    value = item,
                    expires = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null
                };

                var json = JsonSerializer.Serialize(payload);
                await _js.InvokeVoidAsync("localStorage.setItem", cacheKey, json);

                return item;
            }

            public async Task<T?> CacheGetAsync(object key)
            {
                var cacheKey = GetCacheKey(key);
                var json = await _js.InvokeAsync<string>("localStorage.getItem", cacheKey);

                if (string.IsNullOrEmpty(json))
                    return default;

                var payload = JsonSerializer.Deserialize<CachePayload<T>>(json);

                if (payload?.Expires != null && payload.Expires < DateTime.UtcNow)
                {
                    await CacheInvalidateAsync(key);
                    return default;
                }

                return payload!.Value;
            }


            public async Task CacheInvalidateAsync(object key)
            {
                var cacheKey = GetCacheKey(key);
                await _js.InvokeVoidAsync("localStorage.removeItem", cacheKey);
            }

            private class CachePayload<TValue>
            {
                public TValue? Value { get; set; }
                public DateTime? Expires { get; set; }
            }
        }
    }

}
