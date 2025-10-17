using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.Services;

public interface IPlaylistManager
{
   List<Playlist> Playlists { get; set; }
   Playlist? PlayingPlaylist { get; }
   Task DeletePlaylistAsync(Playlist playlist); 
   Task SavePlaylistAsync(Playlist playlist); 
   Task EditPlaylistAsync(Playlist playlist); 
   Task StartPlaylistAsync(Playlist playlist); 
   Task StopPlaylistAsync(); 
   Playlist ConstructPlaylist(PlaylistData playlistData);
}