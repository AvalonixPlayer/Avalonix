using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.PlaylistManager;

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
