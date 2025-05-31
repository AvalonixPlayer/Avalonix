using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI;

public struct PlaylistData
{
    public string Name { get; set; }
    public List<string> SongsPaths { get; set; }
    public int? Year { get; set; }
    public string? Performer { get; set; }
    public string? Album { get; set; }

    public PlaylistData(string name, List<string> songsPaths, int? year = -1, string? perfomer = null!, string? album = null!)
    {
        Name = name;
        SongsPaths = songsPaths;
        Year = year;
        Performer = perfomer;
        Album = album;
    }
}