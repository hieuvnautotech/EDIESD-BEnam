namespace ESD_EDI_BE.Cache
{
    public interface ICache
    {
        long Del(params string[] key);

        Task<long> DelAsync(params string[] key);


        Task<long> DelByPatternAsync(string pattern);


        bool Exists(string key);


        Task<bool> ExistsAsync(string key);


        string Get(string key);


        T Get<T>(string key);


        Task<string> GetAsync(string key);

        Task<T> GetAsync<T>(string key);


        bool Set(string key, object value);


        bool Set(string key, object value, TimeSpan expire);


        Task<bool> SetAsync(string key, object value);


        Task<bool> SetAsync(string key, object value, TimeSpan expire);

        List<string> GetAllKeys();
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem);
        Task<bool> PingAsync();
    }
}
