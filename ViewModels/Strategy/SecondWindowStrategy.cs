using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels.Strategy;

public interface ISecondWindowStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
}

public interface IPlayableWindowStrategy : ISecondWindowStrategy
{
    Task ExecuteAsync(IPlayable playlist);
}


