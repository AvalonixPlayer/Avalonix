using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Services.CacheManager;
using TagLib.Riff;

namespace Avalonix.Model.Media.Track;

public class Track
{
    public TrackMetadata Metadata = new();
    public TrackData TrackData;

    private readonly ICacheManager _cacheManager;

    public Track(string path, ICacheManager cacheManager)
    {
        TrackData = new TrackData(path);
        _cacheManager = cacheManager;
    }

    public async Task FillTrackMetaData()
    {
        if (_cacheManager.TracksMetadataCache != null)
        {
            Console.WriteLine("Cache exists");
            var pair = _cacheManager.TracksMetadataCache.Find(kvp => kvp.Key == TrackData.Path);
            if (string.IsNullOrEmpty(pair.Key))
            {
                Console.WriteLine("Is empty ");

                await Metadata.FillTrackMetaData(TrackData.Path);
                var newCache = _cacheManager.TracksMetadataCache;
                newCache.Add(new KeyValuePair<string, TrackMetadata>(TrackData.Path, Metadata));
                await _cacheManager.SetTracksMetadataCacheAsync(newCache);
            }
            else
            {
                Console.WriteLine("Is not empty");

                Metadata = pair.Value;
            }
        }
        else
        {
            Console.WriteLine("Cache empty");

            await Metadata.FillTrackMetaData(TrackData.Path);
            var newCache = new List<KeyValuePair<string, TrackMetadata>> { new(TrackData.Path, Metadata) };
            await _cacheManager.SetTracksMetadataCacheAsync(newCache);
        }
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}