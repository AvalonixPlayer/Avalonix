using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.PlaylistManager;

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

public class SelectAndPlayPlaylistWindowStrategy(IPlayablesManager playablesManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "to play";
    public string ActionButtonText => "Playlist name to play";

    public async Task ExecuteAsync(IPlayable playlist) => await playablesManager.StartPlayable((playlist as Playlist)!);
}

public class SelectAndEditPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "to edit";
    public string ActionButtonText => "Playlist name to edit";

    public async Task ExecuteAsync(IPlayable playlist) => await playlistManager.EditPlaylist((playlist as Playlist)!);

}
