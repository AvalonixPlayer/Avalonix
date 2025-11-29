using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Playlist;

public class PlaylistData
{
    public string Name = string.Empty;
    public List<string> TracksPaths = [];
    public string? ObservingDirectoryPath = string.Empty;
}