using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI;

public struct SongData
{
    public string Name { get; set; }
    public string Path { get; set; }
    public int? Year { get; set; }
    public SongData(string name, string path, int? year = null)
    {
        Name = name;
        Path = path;
        Year = year;
    }
}