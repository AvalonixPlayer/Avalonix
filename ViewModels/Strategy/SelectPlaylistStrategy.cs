using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public class SelectAndDeletePlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlaylistWindowStrategy
{
    public string WindowTitle => " to delete";
    public string ActionButtonText => "Playlist name to delete";

    public Task ExecuteAsync(Playlist playlist)
    {
        playlistManager.DeletePlaylist(playlist);
        return Task.CompletedTask;
    }
}

public class SelectAndPlayPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlaylistWindowStrategy
{
    public string WindowTitle => "to play";
    public string ActionButtonText => "Playlist name to play";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.StartPlaylist(playlist);
}

public class SelectAndEditPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlaylistWindowStrategy
{
    public string WindowTitle => "to edit";
    public string ActionButtonText => "Playlist name to edit";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist);

}
