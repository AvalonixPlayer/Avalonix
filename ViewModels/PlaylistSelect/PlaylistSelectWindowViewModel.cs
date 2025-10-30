using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlaylistSelect;

public class PlaylistSelectWindowViewModel(IPlaylistManager playlistManager, ISecondWindowStrategy strategy) : ViewModelBase, IPlaylistSelectWindowViewModel
{
    public ISecondWindowStrategy Strategy { get; } = strategy;
    public async Task<List<Playlist>> GetPlaylists() => await playlistManager.GetAllPlaylists();
    public List<Playlist> SearchPlaylists(string text, List<Playlist> playlists) =>
        string.IsNullOrWhiteSpace(text) ? playlists : playlists.
            Where(item => item.Name.
                Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task ExecuteAction(Playlist playlist) => await Strategy.ExecuteAsync(playlist);
}