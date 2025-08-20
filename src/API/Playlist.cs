using System;
using System.Collections.Generic;

namespace Avalonix.API;

[Serializable]
public struct PlaylistData(string name, List<TrackData> songs)
{ 
    public string Name => name;
    public List<TrackData> Songs => songs;
}