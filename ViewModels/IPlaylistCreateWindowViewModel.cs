using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.ViewModels;

public interface IPlaylistCreateWindowViewModel
{
    Task<string[]?> OpenTrackFileDialogAsync(Window parent);
    Task HandlePlaylistSelection(Playlist playlist);
    string WindowTitle { get; }
    string ActionButtonText { get; }
}