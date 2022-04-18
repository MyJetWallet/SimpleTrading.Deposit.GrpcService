using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrpcService.Services
{
    public class LocalCache<TService, TKey, TData> : ILocalCache<TService, TKey, TData> where TData: class
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private SettingModel SettingModel => SettingsReader.SettingsReader.ReadSettings<SettingModel>();
        private ILogger Logger => ServiceLocator.Logger;

        public async Task<TData> GetOrAddAsync(TKey key, Func<Task<TData>> createItem)
        {
            var result = await _cache.GetOrCreateAsync(key, async cacheEntry => 
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SettingModel.PaymentSystemCacheExpirationMin);
                try
                {
                    return await createItem();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message);
                    var old = cacheEntry.Value as TData;
                    if (old is not null)
                    {
                        Logger.Warning("Reuse old settings {@settings}", old);
                        return old;
                    }
                    throw;
                }
            });
            
            return result;
        }
    }

    public interface ILocalCache<TService, TKey, TData>
    {
        Task<TData> GetOrAddAsync(TKey key, Func<Task<TData>> dataLoader);
    }
}
