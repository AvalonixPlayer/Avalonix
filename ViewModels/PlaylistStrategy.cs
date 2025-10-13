using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services;

namespace Avalonix.ViewModels;

public interface IPlaylistActionStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
    Task ExecuteAsync(Playlist playlist);
}

public class PlayPlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to play";
    public string ActionButtonText => "Play";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.StartPlaylistAsync(playlist);
}

public class EditPlaylistStrategy(IPlaylistManager playlistManager): IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to edit";
    public string ActionButtonText => "edit";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylistAsync(playlist);
}

public class DeletePlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to delete";
    public string ActionButtonText => "delete";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.DeletePlaylistAsync(playlist);
}