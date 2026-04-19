using Microsoft.Extensions.Caching.Memory;

namespace sample.NT.KYC.Jibit.Services;

public class JibitCredentialsManagement(IMemoryCache memoryCache)
{
    private const string CacheKey = "JibitCredentialsCacheDto";

    public JibitCredentialsCacheDto? GetCredentialsFromCache()
    {
        var result = memoryCache.TryGetValue(CacheKey, out JibitCredentialsCacheDto? cacheDto);
        if (!result || cacheDto == null) return null;

        return cacheDto;
    }

    public void StoreNewCredentialsInCache(JibitCredentialsCacheDto result)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(2));

        memoryCache.Set(CacheKey, result, cacheEntryOptions);
    }
}