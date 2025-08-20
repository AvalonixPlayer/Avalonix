using System;
using System.Collections.Generic;

namespace Avalonix.API;

[Serializable]
public struct PlaylistData(string name)
{ 
    public string Name => name;
    public List<TrackData> Tracks = new List<TrackData>();

    public void AddTrack(TrackData song)
    {
        Tracks.Add(song);
        DiskManager.SavePlaylistData(this);
    }

    public void RemoveTrack(TrackData song)
    {
        foreach (var track in Tracks)
        {
            if (track.Equals(song))
            {
                Tracks.Remove(track);
            }
        }
    }
}