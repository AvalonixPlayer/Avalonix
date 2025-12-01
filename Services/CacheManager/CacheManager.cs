using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Track;
using Avalonix.Services.DiskManager;

namespace Avalonix.Services.CacheManager;

public class CacheManager : ICacheManager
{
    private readonly IDiskManager _diskManager;

    public List<KeyValuePair<string, TrackMetadata>> TracksMetadataCache { get; private set; } = [];

    public CacheManager(IDiskManager diskManager)
    {
        _diskManager = diskManager;
        LoadTracksMetadataCacheAsync().GetAwaiter();
    }

    public void SetTracksMetadataCache(List<KeyValuePair<string, TrackMetadata>> pairs)
    {
        TracksMetadataCache = pairs;
    }

    public async Task LoadTracksMetadataCacheAsync() =>
        TracksMetadataCache = await _diskManager.LoadTracksMetadataCacheAsync();

    public async Task SaveCacheAsync()
    {
        await _diskManager.SaveTracksMetadataCacheAsync(TracksMetadataCache);
    }
}