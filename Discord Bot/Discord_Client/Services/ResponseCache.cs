using System.Collections.Concurrent;
using Bot_Application.Commands;

namespace Discord_Client.Services
{
    public class ResponseCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public ConcurrentDictionary<string, object> Cache => _cache;

        public ResponseCache() { }

        public void Add(string key, object value)
        {
            _cache.AddOrUpdate(key, value, (k, v) => value);
        }

        public object? Get(string key)
        {
            _cache.TryGetValue(key, out var value);
            return value;
        }

        public async Task ReloadAsync()
        {
            _cache.Clear();
            var newCacheData = await LoadCacheDataAsync();

            foreach (var item in newCacheData)
            {
                _cache[item.Key] = item.Value;
            }
        }

        private async Task<Dictionary<string, object>> LoadCacheDataAsync()
        {
            //todo load data from the backend
            return new Dictionary<string, object>();
        }
    }

    public interface ICache
    {
        void Add(string key, object value);
        object? Get(string key);
        Task ReloadAsync();
    }
}