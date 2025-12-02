using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Track;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskManager;
using Avalonix.Services.DiskWriter;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.CacheManager;

public class CacheManager : ICacheManager
{
    private readonly IDiskWriter _diskWriter;
    private readonly IDiskLoader _diskLoader;
    private readonly ILogger _logger;

    public List<KeyValuePair<string, TrackMetadata>> TracksMetadataCache { get; private set; } = [];

    public CacheManager(IDiskWriter diskWriter, IDiskLoader diskLoader, ILogger logger)
    {
        _diskWriter = diskWriter;
        _diskLoader = diskLoader;
        _logger = logger;
        LoadTracksMetadataCacheAsync().GetAwaiter();
    }

    public void SetTracksMetadataCache(List<KeyValuePair<string, TrackMetadata>> pairs)
    {
        TracksMetadataCache = pairs;
    }

    public async Task SaveCacheAsync()
    {
        _logger.LogDebug("TracksMetadataCache saving");
        await _diskWriter.WriteJsonAsync(TracksMetadataCache, DiskManager.DiskManager.TracksMetadataCachePath);
        _logger.LogDebug("TracksMetadataCache saved");
    }

    public async Task LoadTracksMetadataCacheAsync()
    {
        _logger.LogDebug("TracksMetadataCache loading");
        TracksMetadataCache =
            await _diskLoader.LoadAsyncFromJson<List<KeyValuePair<string, TrackMetadata>>>(DiskManager.DiskManager
                .TracksMetadataCachePath) ?? null!;
        _logger.LogDebug("TracksMetadataCache loaded");
    }
}