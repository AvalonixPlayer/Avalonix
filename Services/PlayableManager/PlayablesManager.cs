using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager.AlbumManager;
using Avalonix.Services.PlayableManager.PlaylistManager;

namespace Avalonix.Services.PlayableManager;

public class PlayablesManager(IPlaylistManager playlistManager, IAlbumManager albumManager) : IPlayablesManager
{
    public Task StartPlayable(IPlayable playable)
    {
        playlistManager.PlayingPlayable?.Stop();
        albumManager.PlayingPlayable?.Stop();
        switch (playable)
        {
            case Playlist:
                playlistManager.StartPlaylist(playable);
                break;
            case Album:
                albumManager.StartAlbum(playable);
                break;
        }
        return Task.CompletedTask;
    }
}