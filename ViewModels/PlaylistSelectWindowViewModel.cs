using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(DiskManager dm) : ViewModelBase
{
    public async Task<List<Playlist>> GetPlaylists() => await dm.GetAllPlaylists();
}