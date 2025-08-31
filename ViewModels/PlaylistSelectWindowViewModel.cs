using System.Collections.Generic;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager idm) : ViewModelBase
{
    public List<Playlist> GetPlaylists() => idm.GetAllPlaylists().GetAwaiter().GetResult();
}