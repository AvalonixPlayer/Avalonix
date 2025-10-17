using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services;

namespace Avalonix.ViewModels.Strategy;

public interface IPlaylistActionStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
    Task ExecuteAsync(Playlist playlist);
}

public class SelectToPlayPlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to play";
    public string ActionButtonText => "Play";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.StartPlaylistAsync(playlist);
}

public class CreatePlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Create a playlist";
    public string ActionButtonText => "Create";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.SavePlaylistAsync(playlist);
}

public class EditPlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Edit playlist";
    public string ActionButtonText => "Save";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylistAsync(playlist);
}

public class SelectToEditPlaylistStrategy(IPlaylistManager playlistManager): IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to edit";
    public string ActionButtonText => "Edit";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylistAsync(playlist);
}

public class SelectToDeletePlaylistStrategy(IPlaylistManager playlistManager) : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to delete";
    public string ActionButtonText => "Delete";
    
    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.DeletePlaylistAsync(playlist);
}