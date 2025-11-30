using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Track;

namespace Avalonix.Services.CacheManager;

public interface ICacheManager
{
    List<KeyValuePair<string, TrackMetadata>>? TracksMetadataCache { get; }
    Task SetTracksMetadataCacheAsync(List<KeyValuePair<string, TrackMetadata>> pairs);
    Task LoadTracksMetadataCacheAsync();
}