using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Services.AlbumManager;

namespace Avalonix.ViewModels.Strategy;

public class SelectAndPlayAlbumWindowStrategy(IAlbumManager albumManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "Select a album to play";
    public string ActionButtonText => "Album name to play";

    public Task ExecuteAsync(IPlayable album)
    {
        albumManager.StartAlbum((album as Album)!);
        return Task.CompletedTask;
    }
}

public class SelectAndDeleteAlbumWindowStrategy(IAlbumManager albumManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "Select a album to delete";
    public string ActionButtonText => "Album name to delete";


    public Task ExecuteAsync(IPlayable album)
    {
        albumManager.RemoveAlbum((album as Album)!);
        return Task.CompletedTask;
    }
}
