using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public class SelectAndDeletePlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => " to delete";
    public string ActionButtonText => "Playlist name to delete";

    public Task ExecuteAsync(IPlayable playlist)
    {
        playlistManager.DeletePlaylist(playlist as Playlist);
        return Task.CompletedTask;
    }
}

public class SelectAndPlayPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "to play";
    public string ActionButtonText => "Playlist name to play";

    public async Task ExecuteAsync(IPlayable playlist) => await playlistManager.StartPlaylist((playlist as Playlist)!);
}

public class SelectAndEditPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "to edit";
    public string ActionButtonText => "Playlist name to edit";

    public async Task ExecuteAsync(IPlayable playlist) => await playlistManager.EditPlaylist((playlist as Playlist)!);

}
