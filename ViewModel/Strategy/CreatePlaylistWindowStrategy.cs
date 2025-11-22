using System.Threading.Tasks;
using Avalonix.Services.Media;
using Avalonix.Services.Media.Playlist;
using Avalonix.Services.PlayableManager.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public class CreatePlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "Create a new playlist";
    public string ActionButtonText => "Create";

    public async Task ExecuteAsync(IPlayable playlist) => await playlistManager.CreatePlaylist((playlist as Playlist)!);
}

public class EditPlaylistWindowStrategy(IPlaylistManager playlistManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "Edit a new playlist";
    public string ActionButtonText => "Save";

    public async Task ExecuteAsync(IPlayable playlist) => await playlistManager.EditPlaylist((playlist as Playlist)!);
}
