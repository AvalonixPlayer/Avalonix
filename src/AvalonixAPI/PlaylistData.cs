using System.Collections.Generic;

namespace Avalonix.AvalonixAPI;

public struct PlaylistData(
    string name,
    List<SongData> songs,
    int? year = -1,
    string? perfomer = null!,
    string? album = null!)
{
    public string Name { get; set; } = name;
    public List<SongData> Songs { get; set; } = songs;
    public int? Year { get; set; } = year;
    public string? Performer { get; set; } = perfomer;
    public string? Album { get; set; } = album;
}