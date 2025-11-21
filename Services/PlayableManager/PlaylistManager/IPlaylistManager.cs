using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;

namespace Avalonix.Services.PlayableManager.PlaylistManager;

public interface IPlaylistManager : IPlayableManager
{
    Playlist ConstructPlaylist(string title, List<Track> tracks, string? observingDirectory);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    void DeletePlaylist(Playlist playlist);
    Task StartPlaylist(Playlist playlist);
    Task<List<Playlist>> GetAllPlaylists();
}
