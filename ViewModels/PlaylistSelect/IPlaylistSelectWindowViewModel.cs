using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlaylistSelect;

public interface IPlaylistSelectWindowViewModel
{
    Task<List<Playlist>> GetPlaylists();
    List<Playlist> SearchPlaylists(string text, List<Playlist> playlists);
    Task ExecuteAction(Playlist playlist);
    ISecondWindowStrategy Strategy { get; }
}