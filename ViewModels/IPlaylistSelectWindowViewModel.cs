using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public interface IPlaylistSelectWindowViewModel
{
    Task<List<Playlist>> GetPlaylists();
    void SearchPlaylists(string text, List<Playlist> playlists, ref ListBox playlistBox);
}