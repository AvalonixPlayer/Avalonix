using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Services.AlbumManager;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.ItemSelect.Album;

public class AlbumSelectViewModel(IAlbumManager albumManager, IAlbumWindowStrategy strategy) : ViewModelBase, IAlbumSelectViewModel, IItemSelectViewModel
{
    public IAlbumWindowStrategy Strategy { get; } = strategy;
    public Task<List<Models.Media.Album.Album>> GetItem() => Task.FromResult(albumManager.GetAlbums());
    public List<Models.Media.Album.Album> SearchItem(string text, List<Models.Media.Album.Album> playlists) =>
        string.IsNullOrWhiteSpace(text) ? playlists : playlists.
            Where(item => item.Metadata is { AlbumName: not null } && item.Metadata.AlbumName.
                Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task ExecuteAction(Models.Media.Album.Album playlist) => await Strategy.ExecuteAsync(playlist);
}
