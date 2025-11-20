using System.Threading.Tasks;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.AlbumManager;

namespace Avalonix.ViewModels.Strategy;

public class SelectAndPlayAlbumWindowStrategy(IAlbumManager albumManager) : ISecondWindowStrategy, IAlbumWindowStrategy
{
    public string WindowTitle => "Select a album to play";
    public string ActionButtonText => "Album name to play";

    public Task ExecuteAsync(Album album)
    {
        albumManager.StartAlbum(album);
        return Task.CompletedTask;
    }
}

public class SelectAndDeleteAlbumWindowStrategy(IAlbumManager albumManager) : IAlbumWindowStrategy
{
    public string WindowTitle => "Select a album to delete";
    public string ActionButtonText => "Album name to delete";


    public Task ExecuteAsync(Album album)
    {
        albumManager.RemoveAlbum(album);
        return Task.CompletedTask;
    }
}
