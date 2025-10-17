using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.ViewModels.Strategy;

public interface ISecondWindowStrategy
{
    public string WindowTitle { get; }
    public string ActionButtonText { get; }
    
    public Task ExecuteAsync(Playlist playlist);
}

public class CreatePlaylistWindowStrategy : ISecondWindowStrategy
{
    public string WindowTitle => "Create a new playlist";
    public string ActionButtonText => "Create";

    public async Task ExecuteAsync(Playlist playlist)
    {
        
    }
}