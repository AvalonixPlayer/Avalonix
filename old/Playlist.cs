using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Avalonix.API.OLD;


public struct PlaylistData(string name)
{
    [JsonInclude] public string Name => name;
    [JsonInclude] public List<Track> Tracks = [];

    public void AddTrack(Track song)
    {
        Tracks.Add(song);
        //Save();
    }

    public void RemoveTrack(TrackMetadata song)
    {
        foreach (var track in Tracks.Where(track => track.Equals(song)).ToList())
            Tracks.Remove(track);
        //Save();
    }

    //private void Save() => DiskManager.SavePlaylistData(this);
}