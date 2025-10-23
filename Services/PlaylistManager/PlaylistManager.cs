using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlaylistManager;

public class PlaylistManager(IMediaPlayer player, IDiskManager diskManager, ILogger logger) : IPlaylistManager
{
    public Playlist ConstructPlaylist(string title, List<Track> tracks)
    {
        var playlistData = new PlaylistData
        {
            Tracks = tracks,
            LastListen = null,
            Rarity = 0 
        };
        var settings = diskManager.GetSettings().GetAwaiter().GetResult();
        return new Playlist(title, playlistData, player, diskManager, logger, settings);
    }

    public async Task EditPlaylist(Playlist playlist) => await playlist.Save();

    public async Task CreatePlaylist(Playlist playlist) => await playlist.Save();
    public void DeletePlaylist(Playlist playlist) => diskManager.RemovePlaylist(playlist.Name);

    public async Task StartPlaylist(Playlist playlist) => await playlist.Play();
}