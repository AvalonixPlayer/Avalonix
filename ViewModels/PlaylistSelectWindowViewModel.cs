using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager idm) : ViewModelBase
{
    public async Task<List<Playlist>> GetPlaylists() => await idm.GetAllPlaylists();
}