using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlayableSelectViewModel;

public interface IPlayableSelectViewModel
{
    Task<List<Models.Media.Playlist.Playlist>> GetPlaylists();
    List<Models.Media.Playlist.Playlist> SearchItem(string text, List<Models.Media.Playlist.Playlist> playlists);
    Task ExecuteAction(Models.Media.Playlist.Playlist playlist);
    IPlaylistWindowStrategy Strategy { get; }
}
