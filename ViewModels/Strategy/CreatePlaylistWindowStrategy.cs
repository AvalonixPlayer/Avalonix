using System.Threading.Tasks;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public class CreatePlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlaylistWindowStrategy
{
    public string WindowTitle => "Create a new playlist";
    public string ActionButtonText => "Create";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.CreatePlaylist(playlist);
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class EditPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlaylistWindowStrategy
{
    public string WindowTitle => "Edit a new playlist";
    public string ActionButtonText => "Save";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist);
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}
