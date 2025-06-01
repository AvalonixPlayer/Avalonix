using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI;

public struct PlaylistData
{
    public string Name { get; set; }
    public List<SongData> Songs { get; set; }
    public int? Year { get; set; }
    public string? Performer { get; set; }
    public string? Album { get; set; }

    public PlaylistData(string name, List<SongData> songs, int? year = -1, string? perfomer = null!, string? album = null!)
    {
        Name = name;
        Songs = songs;
        Year = year;
        Performer = perfomer;
        Album = album;
    }
}