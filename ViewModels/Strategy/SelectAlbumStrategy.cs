using System.Threading.Tasks;
using Avalonix.Services.Media;
using Avalonix.Services.Media.Album;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.AlbumManager;

namespace Avalonix.ViewModels.Strategy;

public class SelectAndPlayAlbumWindowStrategy(IPlayablesManager playablesManager) : IPlayableWindowStrategy
{
    public string WindowTitle => "Select a album to play";
    public string ActionButtonText => "Album name to play";

    public async Task ExecuteAsync(IPlayable album) => await playablesManager.StartPlayable((album as Album)!);
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
