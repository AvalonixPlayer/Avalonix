using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager.PlaylistManager;

namespace Avalonix.Services.PlayableManager;

public class PlayablesManager : IPlayablesManager
{
    public void StartPlayable(IPlayableManager playableManager)
    {
        if (playableManager is IPlaylistManager)
        {
            (playableManager as IPlaylistManager).StartPlaylist(playableManager.PlayingPlayable as Playlist);
        }
    }
}