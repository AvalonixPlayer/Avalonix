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
}

public interface IPlaylistWindowStrategy : ISecondWindowStrategy
{
    Task ExecuteAsync(Playlist playlist);
}

public interface IAlbumWindowStrategy : ISecondWindowStrategy
{
    Task ExecuteAsync(Album playlist);
}


