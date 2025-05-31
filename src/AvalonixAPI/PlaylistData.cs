using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI;

public struct PlaylistData
{
    public string Name { get; set; }

    public PlaylistData(string name)
    {
        Name = name;
    }
}