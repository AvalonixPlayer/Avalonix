using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalonix.API;

[Serializable]
public struct PlaylistData(string name)
{ 
    public string Name => name;
    public List<TrackData> Tracks = [];

    public void AddTrack(TrackData song)
    {
        Tracks.Add(song);
        DiskManager.SavePlaylistData(this);
    }

    public void RemoveTrack(TrackData song)
    {
        foreach (var track in Tracks.Where(track => track.Equals(song)).ToList())
        {
            Tracks.Remove(track);
        }
    }
}