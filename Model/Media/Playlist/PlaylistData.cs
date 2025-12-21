using System.Collections.Generic;

namespace Avalonix.Model.Media.Playlist;

public class PlaylistData
{
    public string Name = string.Empty;
    public string? ObservingDirectoryPath = string.Empty;
    public List<string> TracksPaths = [];
}