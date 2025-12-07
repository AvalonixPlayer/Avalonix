using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonix.Model.Media.Album;

namespace Avalonix.Model;

public record Statistic
{
    [JsonIgnore] public List<Album> FavouriteAlbums { get; init; }
    [JsonIgnore] public List<string> FavouriteTracks { get; init; }
    [JsonIgnore] public List<string> FavouriteArtists { get; init; }
    
    [JsonInclude] public Dictionary<Album, int> AlbumsStatistics { get; init; }
    [JsonInclude] public Dictionary<string, int> TracksStatistics { get; init; }
    [JsonInclude] public Dictionary<string, int> ArtistsStatistics { get; init; }
}