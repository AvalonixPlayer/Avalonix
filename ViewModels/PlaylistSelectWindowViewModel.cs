using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.Playlist;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager diskManager, IPlaylistActionStrategy actionStrategy)
    : ViewModelBase, IPlaylistSelectWindowViewModel
{
    public string WindowTitle => actionStrategy.WindowTitle;
    public string ActionButtonText => actionStrategy.ActionButtonText;

    public async Task<List<Playlist>> GetPlaylists() => await diskManager.GetAllPlaylists();
    
    public List<Playlist> SearchPlaylists(string text, List<Playlist> playlists) =>
        string.IsNullOrWhiteSpace(text) 
            ? playlists 
            : playlists.Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task HandlePlaylistSelection(Playlist playlist) => await actionStrategy.ExecuteAsync(playlist);
}

