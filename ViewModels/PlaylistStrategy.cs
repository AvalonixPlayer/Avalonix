using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.ViewModels;

public interface IPlaylistActionStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
    Task ExecuteAsync(Playlist playlist);
}

public class PlayPlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to play";
    public string ActionButtonText => "Play";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await playlist.Play();
    }
}

public class EditPlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to edit";
    public string ActionButtonText => "edit";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await Task.CompletedTask; 
    }
}

public class DeletePlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Select a playlist to delete";
    public string ActionButtonText => "delete";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await Task.CompletedTask; 
    }
}