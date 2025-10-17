using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public interface ISecondWindowStrategy
{
    public string WindowTitle { get; }
    public string ActionButtonText { get; }
    
    public Task ExecuteAsync(Playlist playlist);
}

public class CreatePlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Create a new playlist";
    public string ActionButtonText => "Create";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.CreatePlaylist(playlist); 
}

public class EditPlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Edit a new playlist";
    public string ActionButtonText => "Save";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist); 
}

public class SelectAndDeletePlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Delete a playlist";
    public string ActionButtonText => "Delete";

    public Task ExecuteAsync(Playlist playlist)
    {
        playlistManager.DeletePlaylist(playlist);    
        return Task.CompletedTask;
    }
}

public class SelectAndPlayPlaylistWindowStrategy() : ISecondWindowStrategy
{
    public string WindowTitle => "Play a playlist";
    public string ActionButtonText => "Play";

    public async Task ExecuteAsync(Playlist playlist) => await playlist.Play();
}

public class SelectAndEditPlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Edit a playlist";
    public string ActionButtonText => "Edit";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist);    
}