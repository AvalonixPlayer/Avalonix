using System.Threading.Tasks;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public interface IPlaylistActionStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
    Task ExecuteAsync(Playlist playlist);
}

public class PlayPlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Выберите плейлист для воспроизведения";
    public string ActionButtonText => "Воспроизвести";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await playlist.Play();
    }
}

public class EditPlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Выберите плейлист для редактирования";
    public string ActionButtonText => "Редактировать";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await Task.CompletedTask; 
    }
}

public class DeletePlaylistStrategy : IPlaylistActionStrategy
{
    public string WindowTitle => "Выберите плейлист для удаления";
    public string ActionButtonText => "Удалить";
    
    public async Task ExecuteAsync(Playlist playlist)
    {
        await Task.CompletedTask; 
    }
}