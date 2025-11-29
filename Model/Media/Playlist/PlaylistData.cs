using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Playlist;

public class PlaylistData
{
    [JsonInclude] public string Name = string.Empty;
    [JsonInclude] public List<string> TracksPaths { get; set; } = [];
}