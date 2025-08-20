using System;

namespace Avalonix.API;

[Serializable]
public struct PlaylistData(string name)
{ 
    public string Name => name;
}