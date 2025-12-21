using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.CacheManager;

namespace Avalonix.Model.Media.Track;

public class Track(string path, ICacheManager cacheManager)
{
    public TrackMetadata Metadata = new();
    public TrackData TrackData = new(path);

    public async Task FillPrimaryMetaData()
    {
        if (cacheManager.TracksMetadataCache != null)
        {
            var pair = cacheManager.TracksMetadataCache.Find(kvp => kvp.Key == TrackData.Path);
            if (string.IsNullOrEmpty(pair.Key))
            {
                await Metadata.FillPreviouslyMetaData(TrackData.Path);
                var newCache = cacheManager.TracksMetadataCache;
                newCache.Add(new KeyValuePair<string, TrackMetadata>(TrackData.Path, Metadata));
                cacheManager.SetTracksMetadataCache(newCache);
            }
            else
            {
                Metadata = pair.Value;
            }
        }
        else
        {
            await Metadata.FillPreviouslyMetaData(TrackData.Path);
            var newCache = new List<KeyValuePair<string, TrackMetadata>> { new(TrackData.Path, Metadata) };
            cacheManager.SetTracksMetadataCache(newCache);
        }
    }

    public async Task FillSecondaryMetaData()
    {
        await Metadata.FillSecondaryMetaData(path);
    }

    [Obsolete("Obsolete")]
    public async Task RewriteMetaData(TrackMetadata metadata)
    {
        await Metadata.RewriteTags(TrackData.Path, metadata);
    }

    public void IncreaseRarity(int rarity)
    {
        TrackData.Rarity += rarity;
    }

    public void UpdateLastListenDate()
    {
        TrackData.LastListen = DateTime.Now.TimeOfDay;
    }
}