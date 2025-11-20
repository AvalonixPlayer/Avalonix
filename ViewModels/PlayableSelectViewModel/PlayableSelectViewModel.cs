using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;
using Avalonix.ViewModels.Strategy;
using TagLib.Ape;

namespace Avalonix.ViewModels.PlayableSelectViewModel;

public class PlayableSelectViewModel(IPlaylistManager playlistManager, IPlaylistWindowStrategy strategy) : ViewModelBase, IPlayableSelectViewModel
{
    public  Strategy { get; } = strategy;
    public async Task<List<Playlist>> GetPlaylists() => await playlistManager.GetAllPlaylists();
    public List<Playlist> SearchItem(string text, List<Playlist> playlists) =>
        string.IsNullOrWhiteSpace(text) ? playlists : playlists.
            Where(item => item.Name.
                Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task ExecuteAction(Item playable) => await Strategy.ExecuteAsync(playable);
}
