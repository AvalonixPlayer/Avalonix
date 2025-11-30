using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Avalonix.Services.CacheManager;

public class CacheManager : ICacheManager
{
    private readonly IMemoryCache _cache;

    public CacheManager(IMemoryCache cache) =>
        _cache = cache;
}