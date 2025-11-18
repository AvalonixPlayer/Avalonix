using System.Threading.Tasks;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.AlbumManager;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public interface ISecondWindowStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
    
    Task ExecuteAsync(Playlist playlist);
    Task ExecuteAsync(Album playlist);
}

public class CreatePlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Create a new playlist";
    public string ActionButtonText => "Create";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.CreatePlaylist(playlist);
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class EditPlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "Edit a new playlist";
    public string ActionButtonText => "Save";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist);
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class SelectAndDeletePlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => " to delete";
    public string ActionButtonText => "Playlist name to delete";

    public Task ExecuteAsync(Playlist playlist)
    {
        playlistManager.DeletePlaylist(playlist);    
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class SelectAndPlayPlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "to play";
    public string ActionButtonText => "Playlist name to play";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.StartPlaylist(playlist); 
    
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class SelectAndEditPlaylistWindowStrategy(IPlaylistManager playlistManager) : ISecondWindowStrategy
{
    public string WindowTitle => "to edit";
    public string ActionButtonText => "Playlist name to edit";

    public async Task ExecuteAsync(Playlist playlist) => await playlistManager.EditPlaylist(playlist);    
    
    public Task ExecuteAsync(Album playlist) =>
        throw new System.NotImplementedException();
}

public class SelectAlbumWindowStrategy(IAlbumManager albumManager) : ISecondWindowStrategy
{
    public string WindowTitle => "to play";
    public string ActionButtonText => "Album name to play";

    public async Task ExecuteAsync(Playlist playlist) =>
        throw new System.NotImplementedException();
    
    public async Task ExecuteAsync(Album album)
    {
        albumManager.StartAlbum(album);
    }
}