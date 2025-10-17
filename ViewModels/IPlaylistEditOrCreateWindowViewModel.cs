using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Avalonix.ViewModels;

public interface IPlaylistEditOrCreateWindowViewModel
{
    Task<string[]?> OpenTrackFileDialogAsync(Window parent);
    Task ExecuteActionAsync(string playlistName, List<string> tracksPaths);
}